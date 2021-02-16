#include "common_material.h"
#include "random.h"

#if 0

//-----------------------------------------------------------------------------
//
//  Lambertian surface closest-hit
//
//-----------------------------------------------------------------------------

rtDeclareVariable(float3, diffuse_color, , "Diffuse color assigned to the material");

RT_PROGRAM void diffuse()
{
	float3 world_shading_normal = normalize(rtTransformNormal(RT_OBJECT_TO_WORLD, shading_normal));
	float3 world_geometric_normal = normalize(rtTransformNormal(RT_OBJECT_TO_WORLD, geometric_normal));
	float3 ffnormal = faceforward(world_shading_normal, -current_ray.direction, world_geometric_normal);

	//float3 hitpoint = current_ray.origin + hit_distance * current_ray.direction;

	//
	// Generate a reflection current_ray.  This will be traced back in current_ray-gen.
	//
	prd_radiance.origin = hit_position;

	float z1 = rnd(prd_radiance.seed);
	float z2 = rnd(prd_radiance.seed);
	float3 p;
	cosine_sample_hemisphere(z1, z2, p);
	optix::Onb onb(ffnormal);
	onb.inverse_transform(p);
	prd_radiance.direction = p;

	// NOTE: f/pdf = 1 since we are perfectly importance sampling lambertian
	// with cosine density.
	prd_radiance.attenuation = prd_radiance.attenuation * diffuse_color;
	prd_radiance.countEmitted = false;

	//
	// Next event estimation (compute direct lighting).
	//
	unsigned int num_lights = lights.size();
	float3 result = make_float3(0.0f);

	for (int i = 0; i < num_lights; ++i)
	{
		// Choose random point on light
		ParallelogramLight light = lights[i];
		const float z1 = rnd(current_prd.seed);
		const float z2 = rnd(current_prd.seed);
		const float3 light_pos = light.corner + light.v1 * z1 + light.v2 * z2;

		// Calculate properties of light sample (for area based pdf)
		const float  Ldist = length(light_pos - hitpoint);
		const float3 L = normalize(light_pos - hitpoint);
		const float  nDl = dot(ffnormal, L);
		const float  LnDl = dot(light.normal, L);

		// cast shadow ray
		if (nDl > 0.0f && LnDl > 0.0f)
		{
			PerRayData_pathtrace_shadow shadow_prd;
			shadow_prd.inShadow = false;
			// Note: bias both ends of the shadow ray, in case the light is also present as geometry in the scene.
			Ray shadow_ray = make_Ray(hitpoint, L, SHADOW_RAY_TYPE, scene_epsilon, Ldist - scene_epsilon);
			rtTrace(top_object, shadow_ray, shadow_prd);

			if (!shadow_prd.inShadow)
			{
				const float A = length(cross(light.v1, light.v2));
				// convert area based pdf to solid angle
				const float weight = nDl * LnDl * A / (M_PIf * Ldist * Ldist);
				result += light.emission * weight;
			}
		}
	}

	prd_radiance.radiance = result;
}

#endif

rtDeclareVariable(float3, Kd, , ) = { 0.9f, 0.9f, 0.9f };
rtDeclareVariable(float3, geometry_color, , ) = { 1.0f, 1.0f, 1.0f };

rtDeclareVariable(float4, light_direction, , ) = { -1.0f, -0.75f, 0.5f };
rtDeclareVariable(float3, light_color, , ) = { 1.0f, 1.0f, 1.0f };
rtDeclareVariable(float3, light_v0, , ) = { 1.0f, 0.0f, 0.0f };
rtDeclareVariable(float3, light_v1, , ) = { 0.0f, 1.0f, 0.0f };
//rtBuffer<DirectionalLight> light_buffer;

RT_PROGRAM void any_hit_shadow()
{
	prd_shadow.attenuation = make_float3(0.0f);
	rtTerminateRay();
}

// Note: both the hemisphere and direct light sampling below use pure random numbers to avoid any patent issues.
// Stratified sampling or QMC would improve convergence.  Please keep this in mind when judging noise levels.

RT_PROGRAM void closest_hit_white()
{
    prd_radiance.result = make_float3(1.0f);
}

RT_PROGRAM void closest_hit_radiance()
{
	const float3 world_shading_normal = normalize(rtTransformNormal(RT_OBJECT_TO_WORLD, shading_normal));
	const float3 world_geometric_normal = normalize(rtTransformNormal(RT_OBJECT_TO_WORLD, geometric_normal));
	const float3 ffnormal = faceforward(world_shading_normal, -current_ray.direction, world_geometric_normal);

	const float z1 = rnd(prd_radiance.seed);
	const float z2 = rnd(prd_radiance.seed);

	float3 w_in;
	cosine_sample_hemisphere(z1, z2, w_in);
	const Onb onb(ffnormal);
	onb.inverse_transform(w_in);
	const float3 fhp = rtTransformPoint(RT_OBJECT_TO_WORLD, front_hit_position);

	prd_radiance.origin = front_hit_position;
	prd_radiance.direction = w_in;
	prd_radiance.attenuation *= Kd * geometry_color;

	// Add direct light sample weighted by shadow term and 1/probability.
	// The pdf for a directional area light is 1/solid_angle.

	const float3 light_center = fhp + make_float3(light_direction.x);
	const float light_radius = light_direction.w;
	const float r1 = rnd(prd_radiance.seed);
	const float r2 = rnd(prd_radiance.seed);
	const float2 disk_sample = square_to_disk(make_float2(r1, r2));
	const float3 jittered_pos = light_center
		+ light_radius * disk_sample.x * light_v0
		+ light_radius * disk_sample.y * light_v1;
	const float3 L = normalize(jittered_pos - fhp);

	const float NdotL = dot(ffnormal, L);
	if (NdotL > 0.0f)
	{
		PerRayData_shadow shadow_prd;
		shadow_prd.attenuation = make_float3(1.0f);
		Ray shadow_ray(fhp, L, SHADOW_RAY_TYPE, 0.0f);
		rtTrace(top_object, shadow_ray, shadow_prd);

		const float solid_angle = light_radius * light_radius * M_PIf;
		prd_radiance.radiance += NdotL * light_color * solid_angle * shadow_prd.attenuation;
	}
}
