#pragma once
#include "stdafx.h"
#include "ConstDef.h"


IppStatus saturate(Ipp16u* buffer, int width, int height, int channels, unsigned short maxValue);
Ipp16u* get_border(const Ipp16u* buffer, Ipp32s width, Ipp32s height, Ipp32s channels, Ipp32s maskSize, 
				  Ipp32s* pStep, IppiPoint* pAnchor);

const Ipp32s* get_filter_kernel(FilterType type, int maskSize);

list<vector<POINT>> find_contours16U(unsigned short* buffer, int width, int height);
int get_next(const unsigned short* buffer, int width, int height,int x, int y, int last);
bool point_on_contour(vector<POINT> contour, RECT bound, POINT point);
int contour_area(vector<POINT> contour);
RECT get_bound_rect(vector<POINT> contour);