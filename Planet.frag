#version 430

in vec3 vs_Normal;
in float vs_Displacement;

out vec3 colour;

void main()
{
	colour = vec3(0.5, 0.0, abs(vs_Displacement));
}