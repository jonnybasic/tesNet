//#include "common.h"
//#include <optixu/optixu_aabb_namespace.h>
#include <optix_world.h>

using namespace optix;

rtDeclareVariable(float3, geometric_normal,   attribute geometric_normal,   "normal to the geometry");
rtDeclareVariable(float3, shading_normal,     attribute shading_normal,     "normal used for shading");
rtDeclareVariable(float3, texcoord,           attribute texcoord,           "polar coordinates");
//rtDeclareVariable(float3, front_hit_position, attribute front_hit_position, "intersection front position");
//rtDeclareVariable(float3, back_hit_position,  attribute back_hit_position,  "intersection back position");

rtDeclareVariable(Ray, current_ray, rtCurrentRay, "built-in access to ray");
rtDeclareVariable(float, scene_epsilon, , );

// Adapted from NVIDIA to be a unit sphere at the origin
template<bool use_robust_method>
static __device__ void intersect_single(void)
{
	static const float radius = 1.0f;

	float3 O = current_ray.origin;
	float  l = 1 / length(current_ray.direction);
	float3 D = current_ray.direction * l;
	float b = dot(O, D);
    // radius*radius = radius
	float c = dot(O, O) - radius;
	float disc = b * b - c;
	if (disc > 0.0f)
	{
		float sdisc = sqrtf(disc);
		float root1 = (-b - sdisc);
		bool do_refine = false;
		float root11 = 0.0f;
        // 10.0f*radius = 10.0f
		if (use_robust_method && fabsf(root1) > 10.0f)
		{
			do_refine = true;
		}
		if (do_refine)
		{
			float3 O1 = O + root1 * D;
			b = dot(O1, D);
            // radius*radius = radius
			c = dot(O1, O1) - radius;
			disc = b * b - c;
			if (disc > 0.0f)
			{
				rtPrintf("us: refined\n");
				sdisc = sqrtf(disc);
				root11 = (-b - sdisc);
			}
		}
		bool second_check = true;
		if (rtPotentialIntersection((root1 + root11) * l))
		{
			const float3 hit_position = (O + (root1 + root11) * D);
			shading_normal = geometric_normal = hit_position;
			//const float3 offset = shading_normal * scene_epsilon;
			//front_hit_position = hit_position + offset;
			//back_hit_position = hit_position - offset;
			float3 polar = cart_to_pol(geometric_normal);
			texcoord = make_float3(polar.x * 0.5f * M_1_PIf,
				                  (polar.y + M_PI_2f) * M_1_PIf,
                                   polar.z);
			rtPrintf("us: outside -> hit inside: d=%f, s=%f, t=%f\n",
					 ((root1 + root11) * l), texcoord.x, texcoord.y);
			if (rtReportIntersection(0))
			{
				second_check = false;
			}
		}
		if (second_check)
		{
			//float root2 = (-b + sdisc);
            float root2 = (-b + sdisc) + (do_refine ? root1 : 0);
			if (rtPotentialIntersection(root2 * l))
			{
				//const float3 hit_position = (O + (root1 + root11) * D);
                const float3 hit_position = (O + root2 * D);
				shading_normal = geometric_normal = hit_position;
				//const float3 offset = shading_normal * scene_epsilon;
				//front_hit_position = hit_position - offset;
				//back_hit_position = hit_position + offset;
				float3 polar = cart_to_pol(geometric_normal);
				texcoord = make_float3(polar.x * 0.5f * M_1_PIf,
					                  (polar.y + M_PI_2f) * M_1_PIf,
                                       polar.z);
				rtPrintf("us: inside -> hit outside: d=%f, s=%f, t=%f\n",
						 (root2 * l), texcoord.x, texcoord.y);
				rtReportIntersection(0);
			}
		}
	}
}

RT_PROGRAM void intersect(int primIdx)
{
	intersect_single<true>();
}

RT_PROGRAM void intersect_fast(int primInx)
{
	intersect_single<false>();
}

RT_PROGRAM void bounds(int primIdx, float results[6])
{
	static const float radius = 1.0f;
	// A unit sphere with a radius of 1.0
	Aabb* aabb = (Aabb*)results;
	aabb->m_min = make_float3(-radius);
	aabb->m_max = make_float3(radius);
}
