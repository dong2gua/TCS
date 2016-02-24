#include "stdafx.h"
#include "Utils.h"

const Ipp32s* get_filter_kernel(FilterType type, int maskSize)
{
	switch (type)
	{
	case HiGauss:
		{
			switch (maskSize)
			{
			case 5:
				return HiGauss5Kernel;
			case 7:
				return HiGauss7Kernel;
			case 9:
				return HiGauss9Kernel;
			}
		}
	case HiPass:
		{
			switch (maskSize)
			{
			case 3:
				return HiPass3Kernel;
			case 5:
				return HiPass5Kernel;
			case 7:
				return HiPass7Kernel;
			}
		}
	case HorizontalEdge:
		{
			switch (maskSize)
			{
			case 3:
				return HorizontalEdge3Kernel;
			case 5:
				return HorizontalEdge5Kernel;
			case 7:
				return HorizontalEdge7Kernel;
			}
		}
	case Laplace:
		{
			switch (maskSize)
			{
			case 3:
				return Laplace3Kernel;
			case 5:
				return Laplace5Kernel;
			case 7:
				return Laplace7Kernel;
			}
		}

	case LowPass:
		{
			switch (maskSize)
			{
			case 3:
				return LowPass3Kernel;
			case 5:
				return LowPass5Kernel;
			case 7:
				return LowPass7Kernel;
			}
		}
	case VerticalEdge:
		{
			switch (maskSize)
			{
			case 3:
				return VerticalEdge3Kernel;
			case 5:
				return VerticalEdge5Kernel;
			case 7:
				return VerticalEdge7Kernel;
			}
		}
	case Fish:
		{
			switch (maskSize)
			{
			case 11:
				return Fish11Kernel;
			}
		}

	case FishB:
		{
			switch (maskSize)
			{
			case 11:
				return FishB11Kernel;
			}
		}

	case FishC:
		{
			switch (maskSize)
			{
			case 11:
				return FishC11Kernel;
			}
		}
	}
	return nullptr;
}

Ipp16u* get_border(const Ipp16u* buffer, Ipp32s width, Ipp32s height, Ipp32s channels, Ipp32s maskSize,
                  Ipp32s* pStep, IppiPoint* pAnchor)
{
	const int anchorX = maskSize / 2;
	const int anchorY = anchorX;
	(*pAnchor).x = anchorX;
	(*pAnchor).y = anchorY;
	const int offsetTop = anchorY;
	const int offsetLeft = anchorX;
	const int offsetRight = maskSize - anchorX - 1;
	const int offsetBottom = maskSize - anchorY - 1;
	const int elementSize = sizeof(Ipp16u);
	const IppiSize roiWithBorder = {width + offsetLeft + offsetRight, height + offsetTop + offsetBottom};
	const IppiSize roi = {width, height};
	const int srcStep = width * elementSize;
	int stepWithBorder = 0;
	Ipp16u* bufferWithBorder = ippiMalloc_16u_C1(roiWithBorder.width, roiWithBorder.height, &stepWithBorder);
	ippiCopyConstBorder_16u_C1R(buffer, srcStep, roi, bufferWithBorder,
	                            stepWithBorder, roiWithBorder, offsetTop, offsetLeft, 0);
	*pStep = stepWithBorder;
	return bufferWithBorder;
}

IppStatus saturate(Ipp16u* buffer, int width, int height, int channels, unsigned short maxValue)
{
	const IppiSize roi = {width, height};
	const int step = width * channels * sizeof(unsigned short);
	int maskStep = 0;
	Ipp8u* mask = ippiMalloc_8u_C1(width, height, &maskStep);
	IppStatus status;
	switch (channels)
	{
	case 1:
		{
			status = ippiCompareC_16u_C1R(buffer, step, maxValue, mask, maskStep, roi, ippCmpGreater);
			if (status == ippStsNoErr)
				status = ippiSet_16u_C1MR(maxValue, buffer, step, roi, mask, maskStep);
			break;
		}
	case 3:
		{
			Ipp16u* pDst[3];
			int dstStep = 0;
			pDst[0] = ippiMalloc_16u_C1(width, height, &dstStep);
			pDst[1] = ippiMalloc_16u_C1(width, height, &dstStep);
			pDst[2] = ippiMalloc_16u_C1(width, height, &dstStep);
			status = ippiCopy_16u_C3P3R(buffer, step, pDst, dstStep, roi);
			if (status == ippStsNoErr)
			{
				for (int i = 0; i < channels; i++)
				{
					ippiCompareC_16u_C1R(pDst[i], dstStep, maxValue, mask, maskStep, roi, ippCmpGreater);
					ippiSet_16u_C1MR(maxValue, pDst[i], dstStep, roi, mask, maskStep);
				}
				status = ippiCopy_16u_P3C3R(pDst, dstStep, buffer, step, roi);
				for (int i = 0; i < 3; i++)
					ippiFree(pDst[i]);
				break;
			}
		}
	default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	ippiFree(mask);
	return status;
}

list<vector<POINT>> find_contours16U(unsigned short* buffer, int width, int height)
{
	vector<POINT> points;
	list<vector<POINT>> contours;
	vector<RECT> bound_rects;
	// Search for starting positions
	for (int sy = 0; sy < height - 1; sy++)
	{
		for (int sx = 0; sx < width - 1; sx++)
		{
			if (buffer[sx + sy * width] == 0) continue;

			if ((sx != 0 && sy != 0 && buffer[sx - 1 + (sy - 1) * width] != 0) ||
				(sx != 0 && buffer[sx - 1 + sy * width] != 0) ||
				(sy != 0 && (buffer[sx + (sy - 1) * width] != 0 || buffer[sx + 1 + (sy - 1) * width] != 0)))
				continue;

			// Prepare to track contour 
			int x = sx;
			int y = sy;
			POINT pt = {x, y};

			// Check if the blob containing this point already contoured
			bool exist = false;
			int index = 0;
			for(auto it = contours.begin(); it!=contours.end(); ++it, index++)
			{
				vector<POINT> contour = *it;
				exist = point_on_contour(contour, bound_rects[index], pt);
				if(exist) break;
			}

			if (exist) continue;

			points.push_back(pt); // start of a contour
			int last = 0;
			int next = get_next(buffer, width, height, x, y, last);

			// Track contour counter clockwise
			while (true)
			{
				x = x + Dx[next];
				y = y + Dy[next];

				if (x < 0 || y < 0 || buffer[x + y * width] == 0)
					break;

				if (x == sx && y == sy) // complete a contour
				{
					if (points.size() > 5)
					{
						contours.push_back(points);
						RECT bound = get_bound_rect(points);
						bound_rects.push_back(bound);				
					}
					break;
				}

				POINT point = {x,y};
				points.push_back(point);

				last = (next + 4) % 8;
				next = get_next(buffer, width, height, x, y, last);
			}

			points.clear();
		}
	}

	return contours;
}


int get_next(const unsigned short* buffer, int width, int height, int x, int y, int last)
{
	int next = (last + 2) % 8;
	int nx = x + Dx[next];
	int ny = y + Dy[next];
	while (next != last &&
		(nx < 0 || nx >= width ||
			(ny < 0) || ny >= height ||
			buffer[nx + ny * width] == 0))
	{
		next = (next + 1) % 8;
		nx = x + Dx[next];
		ny = y + Dy[next];
	}
	return next;
}



int contour_area(vector<POINT> contour)
{
	int area = 0;
    // create an array of n+2 vertices with V[n]=V[0] and V[n+1]=V[1]				
    size_t n = contour.size();
	vector<POINT> pts(n+2);
    for (size_t m = 0; m < n; m++)
        pts[m] = contour[m];

    pts[n] = contour[0];
    pts[n + 1] = contour[1];

    int i, j, k;

    for (i = 1, j = 2, k = 0; i <= n; i++, j++, k++)
        area += pts[i].x * (pts[j].y - pts[k].y);

    area /= 2;

    if (area < 0)
        area = -area;
    return area;
}

bool point_on_contour(vector<POINT> contour, RECT bound, POINT point)
{
	if(point.x < bound.left || point.x > bound.right 
		|| point.y < bound.top || point.y > bound.bottom)
		return false;
	for(auto it = contour.begin(); it != contour.end(); ++it)
	{
		if(point.x==(*it).x&&point.y == (*it).y) return true;
	}
	return false;
}

RECT get_bound_rect(vector<POINT> contour)
{
	if(contour.size()==0)
	{
		RECT empty = {0,0,0,0};
		return empty;
	}
	long x_min = contour[0].x;
	long x_max = x_min;
	long y_min = contour[0].y;
	long y_max = y_min;
	for(auto it = contour.begin();it!=contour.end();++it)
	{
		POINT point = *it;
		if(point.x<x_min) x_min = point.x;
		else if(point.x>x_max) x_max = point.x;
		if(point.y<y_min) y_min = point.y;
		else if(point.y>y_max) y_max = point.y;
	}

	RECT rect = {x_min,y_min, x_max, y_max};
	return rect;
}
