#version 430

#pragma name PlanetShader

layout(location = 0) uniform mat4 MVP;
uniform float MaxDisplacement;

layout(location = 0) in vec3 Vertex;
layout(location = 1) in vec3 Normal;
layout(location = 2) in float Displacement;

smooth out vec3 vs_Normal;
smooth out float vs_Displacement;

void main()
{
	gl_Position = MVP * vec4(Vertex, 1.0);
	
	vs_Normal = Normal;
	vs_Displacement = Displacement / MaxDisplacement;
}