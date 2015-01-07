//Should contain one shader that can be used for all CGFX files, using dmp uniforms
struct FragmentMaterial
{
	vec4 ambient;
	vec4 emission;
	vec4 diffuse;
	vec4 specular0;
	vec4 specular1;
};

struct TevCombiner
{
	int combineRgb;
	int combineAlpha;
	int srcRgb;
	int srcAlpha;
	int operandRgb;
	int operandAlpha;
	int bufferInput;
	float scaleRgb;
	float scaleAlpha;
	vec4 constRgba;
	vec4 bufferColor;
};

uniform sampler2D dmp_Texture[4];

uniform FragmentMaterial dmp_FragmentMaterial;
uniform TevCombiner dmp_TexEnv[6];

vec4 previous = vec4(1.0);

vec4 buf[5];

vec4 bufin;
vec4 constRgba;

void main()
{
	buf[0] = dmp_TexEnv[0].bufferColor;
	for(int i = 0; i < 6; i++)
	{
		if (i > 0 && i < 5)
		{
			if (dmp_TexEnv[i].bufferInput & 1) buf[i].rgb = previous.rgb;
			else buf[i].rgb = buf[i-1].rgb;
			if ((dmp_TexEnv[i].bufferInput >> 1) & 1) buf[i].a = previous.a;
			else buf[i].a = buf[i-1].a;
		}

		if (i > 0) bufin = buf[i - 1];
		constRgba = dmp_TexEnv[i].constRgba;

		vec4 inputs[3];
		for(int i = 0; i < 3; i++)
		{
			int srcRgb = (dmp_TexEnv[i].srcRgb >> (i * 4)) & 0xF;
			int opRgb = (dmp_TexEnv[i].operandRgb >> (i * 4)) & 0xF;
			inputs[i].rgb = getOperandRgb(opRgb, getSource(srcRgb));
		}
		for(int i = 0; i < 3; i++)
		{
			int srcAlpha = (dmp_TexEnv[i].srcAlpha >> (i * 4)) & 0xF;
			int opAlpha = (dmp_TexEnv[i].operandAlpha >> (i * 4)) & 0xF;
			inputs[i].a = getOperandAlpha(opAlpha, getSource(srcAlpha));
		}
		
		previous = clamp(vec4(
				getCombinedRgb(dmp_TexEnv[i].combineRgb, inputs[0], inputs[1], inputs[2]) * vec3(dmp_TexEnv[i].scaleRgb),
				getCombinedAlpha(dmp_TexEnv[i].combineAlpha, inputs[0], inputs[1], inputs[2]) * dmp_TexEnv[i].scaleAlpha),
				0.0, 1.0);
	}
	gl_FragColor = previous;
}

vec4 getSource(int src)
{
	switch(src)
	{
		case 0: return gl_Color;
		case 1: return vec4(0.5, 0.5, 0.5, 1.0);//tempoarly
		case 2: return vec4(0.5, 0.5, 0.5, 1.0);//tempoarly
		case 3: return texture2D(dmp_Texture[0], gl_TexCoord[0].st);
		case 4: return texture2D(dmp_Texture[1], gl_TexCoord[1].st);
		case 5: return texture2D(dmp_Texture[2], gl_TexCoord[2].st);
		//proctex not supported!
		//case 6: return texture2D(dmp_Texture[3], gl_TexCoord[3].st);
		case 13: return bufin;
		case 14: return constRgba;
		case 15: return previous;
	}
	return vec4(0.0, 0.0, 0.0, 1.0);
}

vec3 getOperandRgb(int op, vec4 src)
{
	switch(op)
	{
		case 0: return src.rgb;
		case 1: return vec3(1.0) - src.rgb;
		case 2: return src.aaa;
		case 3: return vec3(1.0) - src.aaa;
		case 4: return src.rrr;
		case 5: return vec3(1.0) - src.rrr;
		case 8: return src.ggg;
		case 9: return vec3(1.0) - src.ggg;
		case 12: return src.bbb;
		case 13: return vec3(1.0) - src.bbb;
	}
	return vec3(0);
}

float getOperandAlpha(int op, vec4 src)
{
	switch(op)
	{
		case 0: return src.a;
		case 1: return 1.0 - src.a;
		case 2: return src.r;
		case 3: return 1.0 - src.r;
		case 4: return src.g;
		case 5: return 1.0 - src.g;
		case 6: return src.b;
		case 7: return 1.0 - src.b;
	}
	return 1;
}

vec3 getCombinedRgb(int comb, vec4 a, vec4 b, vec4 c)
{
	switch(comb)
	{
		case 0: return a.rgb;
		case 1: return a.rgb * b.rgb;
		case 2: return a.rgb + b.rgb;
		case 3:	return a.rgb + b.rgb - vec3(0.5);
		case 4:	return a.rgb * c.rgb + b.rgb * (vec3(1.0) - c.rgb);
		case 5:	return a.rgb - b.rgb;
		case 6: 
		case 7: return vec3(4 * ((a.r - 0.5) * (b.r - 0.5) + (a.g - 0.5) * (b.g - 0.5) + (a.b - 0.5) * (b.b - 0.5)));
		case 8: return (a.rgb * b.rgb) + c.rgb;
		case 9: return clamp(a.rgb + b.rgb, 0.0, 1.0) * c.rgb;
	}
	return vec3(0);
}

float getCombinedAlpha(int comb, vec4 a, vec4 b, vec4 c)
{
	switch(comb)
	{
		case 0: return a.a;
		case 1: return a.a * b.a;
		case 2: return a.a + b.a;
		case 3:	return a.a + b.a - 0.5;
		case 4:	return a.a * c.a + b.a * (1.0 - c.a);
		case 5:	return a.a - b.a;
		//not possible?
		//case 6:
		case 7: return 4 * ((a.r - 0.5) * (b.r - 0.5) + (a.g - 0.5) * (b.g - 0.5) + (a.b - 0.5) * (b.b - 0.5));
		case 8: return (a.a * b.a) + c.a;
		case 9: return clamp(a.a + b.a, 0.0, 1.0) * c.a;
	}
	return 1;
}