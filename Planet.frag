#version 430

in vec3 vs_Normal;
in float vs_Displacement;

uniform vec3 LightDir;

out vec3 colour;

float computeDiffuse(vec3 normal) {
	return clamp( dot( normal, -LightDir ), 0,1 );
}

void main()
{
	colour = vec3(max(vs_Displacement, 1.0)) * computeDiffuse(vs_Normal);
}