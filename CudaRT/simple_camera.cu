#include "common.h"

rtDeclareVariable(float3, eye, , );
rtDeclareVariable(float3, U, , );
rtDeclareVariable(float3, V, , );
rtDeclareVariable(float3, W, , );
rtDeclareVariable(float3, bad_color, , ) = { 1.0f, 0.0f, 1.0f };

rtBuffer<uchar4, 2> output_buffer;

rtDeclareVariable(uint2, launch_index, rtLaunchIndex, );
rtDeclareVariable(uint2, launch_dim, rtLaunchDim, );

RT_PROGRAM void pinhole_camera()
{
    float2 d = make_float2(launch_index) / make_float2(launch_dim) * 2.f - 1.f;
    float3 ray_origin = eye;
    float3 ray_direction = normalize(d.x*U + d.y*V + W);

    Ray ray = make_Ray(ray_origin,
                       ray_direction,
                       RADIANCE_RAY_TYPE,
                       scene_epsilon,
                       RT_DEFAULT_MAX);

    PerRayData_radiance prd;
    prd.depth = 0;
    prd.done = false;
    prd.result = make_float3(0.0f);

    // brdf attenuation from surface interaction
    prd.attenuation = make_float3(1.0f);
    // light from a light source or miss program
    prd.radiance = make_float3(0.0f);

    rtTrace(top_object, ray, prd);

    output_buffer[launch_index] = make_color(prd.result);
}

RT_PROGRAM void exception()
{
    rtPrintExceptionDetails();
    output_buffer[launch_index] = make_color(bad_color);
}
