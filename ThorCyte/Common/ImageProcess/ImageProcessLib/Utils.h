#pragma once
#include "stdafx.h"
#include "ConstDef.h"


IppStatus saturate(Ipp16u* buffer, int width, int height, int channels, unsigned short maxValue);
Ipp16u* getBorder(const Ipp16u* buffer, Ipp32s width, Ipp32s height, Ipp32s channels, Ipp32s maskSize, 
				  Ipp32s* pStep, IppiPoint* pAnchor);
const Ipp32s* getFilterKernel(FilterType type, int maskSize);

vector<vector<Point>> findContours16U(unsigned short* buffer, int width, int height);