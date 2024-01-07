#include "maths.h"

#define ease(x) (((x*6-15)*x + 10) * x * x * x)
#define lerp(a, b, t) (a + ((b-a)*t))
#define dot2(v, xm, ym) (v.x * xm + v.y * ym)

const static unsigned char randTab[] =
{
   23, 125, 161, 52, 103, 117, 70, 37, 247, 101, 203, 169, 124, 126, 44, 123, 
   152, 238, 145, 45, 171, 114, 253, 10, 192, 136, 4, 157, 249, 30, 35, 72, 
   175, 63, 77, 90, 181, 16, 96, 111, 133, 104, 75, 162, 93, 56, 66, 240, 
   8, 50, 84, 229, 49, 210, 173, 239, 141, 1, 87, 18, 2, 198, 143, 57, 
   225, 160, 58, 217, 168, 206, 245, 204, 199, 6, 73, 60, 20, 230, 211, 233, 
   94, 200, 88, 9, 74, 155, 33, 15, 219, 130, 226, 202, 83, 236, 42, 172, 
   165, 218, 55, 222, 46, 107, 98, 154, 109, 67, 196, 178, 127, 158, 13, 243, 
   65, 79, 166, 248, 25, 224, 115, 80, 68, 51, 184, 128, 232, 208, 151, 122, 
   26, 212, 105, 43, 179, 213, 235, 148, 146, 89, 14, 195, 28, 78, 112, 76, 
   250, 47, 24, 251, 140, 108, 186, 190, 228, 170, 183, 139, 39, 188, 244, 246, 
   132, 48, 119, 144, 180, 138, 134, 193, 82, 182, 120, 121, 86, 220, 209, 3, 
   91, 241, 149, 85, 205, 150, 113, 216, 31, 100, 41, 164, 177, 214, 153, 231, 
   38, 71, 185, 174, 97, 201, 29, 95, 7, 92, 54, 254, 191, 118, 34, 221, 
   131, 11, 163, 99, 234, 81, 227, 147, 156, 176, 17, 142, 69, 12, 110, 62, 
   27, 255, 0, 194, 59, 116, 242, 252, 19, 21, 187, 53, 207, 129, 64, 135, 
   61, 40, 167, 237, 102, 223, 106, 159, 197, 189, 215, 137, 36, 32, 22, 5  
};

const static Vec2 gradients2D[] = 
{
    { 1.0f, 0.0f},
    {-1.0f, 0.0f},
    { 0.0f, 1.0f},
    { 0.0f,-1.0f},
    { 0.70710678f, 0.70710678f},
    {-0.70710678f, 0.70710678f},
    { 0.70710678f,-0.70710678f},
    {-0.70710678f,-0.70710678f}
};

float perlin2D(Vec3 point, float frequency) 
{
		point *= frequency;
		int ix0 = (int)myfloorf(point.x);
		int iy0 = (int)myfloorf(point.y);
		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tx1 = tx0 - 1.0f;
		float ty1 = ty0 - 1.0f;
		ix0 &= 255;
		iy0 &= 255;
		int ix1 = (ix0 + 1) & 255;
		int iy1 = (iy0 + 1) & 255;
		
		int h0 = randTab[ix0];
		int h1 = randTab[ix1];

		Vec2 g00 = gradients2D[randTab[h0 + iy0] & 7];
		Vec2 g10 = gradients2D[randTab[h1 + iy0] & 7];
		Vec2 g01 = gradients2D[randTab[h0 + iy1] & 7];
		Vec2 g11 = gradients2D[randTab[h1 + iy1] & 7];

		float v00 = dot2(g00, tx0, ty0);
		float v10 = dot2(g10, tx1, ty0);
		float v01 = dot2(g01, tx0, ty1);
		float v11 = dot2(g11, tx1, ty1);
		
		float tx = ease(tx0);
		float ty = ease(ty0);
		return lerp(lerp(v00, v10, tx),lerp(v01, v11, tx), ty) * 1.41421356f;
}
