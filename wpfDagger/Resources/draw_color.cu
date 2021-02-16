
#include "common.h"

rtDeclareVariable(uint2, launch_index, rtLaunchIndex, );

rtBuffer<uchar4, 2>   output_buffer;

rtDeclareVariable(float3, draw_color, , );

RT_PROGRAM void draw_solid_color()
{
	//result_buffer[launch_index] = make_float4(draw_color, 0.f);
	output_buffer[launch_index] = make_color(draw_color);
}
