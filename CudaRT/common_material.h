#pragma once

#include "common.h"

rtDeclareVariable(float2, texcoord,           attribute texcoord,           "texture coordinates");
rtDeclareVariable(float3, geometric_normal,   attribute geometric_normal,   "normal to the geometry");
rtDeclareVariable(float3, shading_normal,     attribute shading_normal,     "normal used for shading");
rtDeclareVariable(float3, front_hit_position, attribute front_hit_position, "intersection front position");
rtDeclareVariable(float3, back_hit_position,  attribute back_hit_position,  "intersection back position");

rtDeclareVariable(Ray, current_ray,    rtCurrentRay,            "built-in access to ray");
rtDeclareVariable(PerRayData_radiance, prd_radiance, rtPayload, "built-in access to ray payload");
rtDeclareVariable(PerRayData_shadow,   prd_shadow, rtPayload,   "built-in access to ray payload");
rtDeclareVariable(float, hit_distance, rtIntersectionDistance,  "built-in intersection distance");
