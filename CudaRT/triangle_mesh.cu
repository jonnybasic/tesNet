/*
 * Copyright (c) 2018, NVIDIA CORPORATION. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *  * Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *  * Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *  * Neither the name of NVIDIA CORPORATION nor the names of its
 *    contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "common.h"
#include <optixu/optixu_aabb_namespace.h>

rtDeclareVariable(float2, texcoord,           attribute texcoord,           "texture coordinates");
rtDeclareVariable(float3, geometric_normal,   attribute geometric_normal,   "normal to the geometry");
rtDeclareVariable(float3, shading_normal,     attribute shading_normal,     "normal used for shading");
rtDeclareVariable(float3, front_hit_position, attribute front_hit_position, "intersection front position");
rtDeclareVariable(float3, back_hit_position,  attribute back_hit_position,  "intersection back position");

rtDeclareVariable(Ray, current_ray,    rtCurrentRay,            "built-in access to ray");

// This is to be plugged into an RTgeometry object to represent
// a triangle mesh with a vertex buffer of triangle soup (triangle list)
// with an interleaved position, normal, texturecoordinate layout.

rtBuffer<float3> vertex_buffer;
rtBuffer<float3> normal_buffer;
rtBuffer<float2> texcoord_buffer;
rtBuffer<int3>   index_buffer;
//rtBuffer<int>    material_buffer;

static __device__ void intersect_primitive(int primIdx)
{
	const int3 v_idx = index_buffer[primIdx];

	const float3 p0 = vertex_buffer[v_idx.x];
	const float3 p1 = vertex_buffer[v_idx.y];
	const float3 p2 = vertex_buffer[v_idx.z];

	// Intersect ray with triangle
	float3 n;
	float  t, beta, gamma;
	if (intersect_triangle(current_ray, p0, p1, p2, n, t, beta, gamma))
	{
		if (rtPotentialIntersection(t))
		{
			geometric_normal = normalize(n);
			if (normal_buffer.size() == 0)
			{
				shading_normal = geometric_normal;
			}
			else
			{
				float3 n0 = normal_buffer[v_idx.x];
				float3 n1 = normal_buffer[v_idx.y];
				float3 n2 = normal_buffer[v_idx.z];
				shading_normal = normalize(n1 * beta + n2 * gamma + n0 * (1.0f - beta - gamma));
			}
			if (texcoord_buffer.size() == 0)
			{
				texcoord = make_float2(0.0f, 0.0f);
			}
			else
			{
				float2 t0 = texcoord_buffer[v_idx.x];
				float2 t1 = texcoord_buffer[v_idx.y];
				float2 t2 = texcoord_buffer[v_idx.z];
				texcoord = (t1 * beta + t2 * gamma + t0 * (1.0f - beta - gamma));
			}
			const float3 hit_position = current_ray.origin + t * current_ray.direction;
			const float3 offset = geometric_normal * scene_epsilon;
			front_hit_position = hit_position + offset;
			back_hit_position = hit_position - offset;

			//rtReportIntersection(material_buffer[primIdx]);
			rtReportIntersection(0);
		}
	}
}

RT_PROGRAM void intersect(int primIdx)
{
	intersect_primitive(primIdx);
}

RT_PROGRAM void bounds(int primIdx, float result[6])
{
	const int3 v_idx = index_buffer[primIdx];

	const float3 v0 = vertex_buffer[v_idx.x];
	const float3 v1 = vertex_buffer[v_idx.y];
	const float3 v2 = vertex_buffer[v_idx.z];
	const float  area = length(cross(v1 - v0, v2 - v0));

	Aabb* aabb = (Aabb*)result;

	if (area > 0.0f && !isinf(area))
	{
		aabb->m_min = fminf(fminf(v0, v1), v2);
		aabb->m_max = fmaxf(fmaxf(v0, v1), v2);
	}
	else
	{
		aabb->invalidate();
	}
}
