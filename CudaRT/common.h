#pragma once

#include <optix.h>
#include <optixu/optixu_vector_types.h>
#include <optixu/optixu_math_namespace.h>

// HACK: Intellisense
#ifndef __CUDACC__
#include <optix_device.h>
// host defined function
static unsigned char __saturatef(float);
#endif

#define RADIANCE_RAY_TYPE 0
#define SHADOW_RAY_TYPE 1

using namespace optix;

// Context variable for scene epsilon
rtDeclareVariable(float, scene_epsilon, , "minimum value representing zero");
rtDeclareVariable(int,   max_depth,     , "maximum traversal depth");
// Scene Root
rtDeclareVariable(rtObject, top_object, , "scene root object");

struct PerRayData_radiance
{
	float3 result;
	float3 radiance;
	float3 attenuation;
	float3 origin;
	float3 direction;
	unsigned int seed;
	int depth;
	int countEmitted;
	int done;
};

struct PerRayData_shadow
{
	bool inShadow;
	float3 attenuation;
};

static __device__ __inline__ uchar4 make_color(const float3& c)
{
    return make_uchar4( static_cast<unsigned char>(__saturatef(c.z)*255.99f),  /* B */
                        static_cast<unsigned char>(__saturatef(c.y)*255.99f),  /* G */
                        static_cast<unsigned char>(__saturatef(c.x)*255.99f),  /* R */
                        255u);                                                 /* A */
}
