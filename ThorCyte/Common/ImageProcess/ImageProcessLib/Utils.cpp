#include "stdafx.h"
#include "Utils.h"

const Ipp32s* getFilterKernel(FilterType type, int maskSize) 
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

Ipp16u* getBorder(const Ipp16u* buffer, Ipp32s width, Ipp32s height, Ipp32s channels, Ipp32s maskSize, 
				  Ipp32s* pStep, IppiPoint* pAnchor)
{
	const int anchorX = maskSize/2;
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
	Ipp16u* bufferWithBorder = ippiMalloc_16u_C1(roiWithBorder.width , roiWithBorder.height, &stepWithBorder);	
	ippiCopyConstBorder_16u_C1R(buffer, srcStep, roi, bufferWithBorder, 
												   stepWithBorder, roiWithBorder, offsetTop, offsetLeft, 0); 
	*pStep = stepWithBorder;
	return bufferWithBorder;
}

IppStatus saturate(Ipp16u* buffer, int width, int height, int channels, unsigned short maxValue)
{
	const IppiSize roi = {width, height};
	const int step = width * channels * sizeof(unsigned short);
	int maskStep  = 0;
	Ipp8u* mask = ippiMalloc_8u_C1(width , height, &maskStep);
	IppStatus status;
	switch (channels)
	{
		case 1:
		{
			status = ippiCompareC_16u_C1R(buffer, step, maxValue, mask, maskStep,roi, ippCmpGreater);
			if(status == ippStsNoErr)
			 	status = ippiSet_16u_C1MR(maxValue, buffer, step, roi, mask, maskStep);
			break;
		}
		case 3:
		{
		
			Ipp16u* pDst[3];
			int dstStep = 0;
			pDst[0] = ippiMalloc_16u_C1(width , height, &dstStep);
			pDst[1] = ippiMalloc_16u_C1(width , height, &dstStep);
			pDst[2] = ippiMalloc_16u_C1(width , height, &dstStep);
			status = ippiCopy_16u_C3P3R(buffer, step, pDst, dstStep, roi);
			if(status==ippStsNoErr)
			{

				for(int i=0; i<channels; i++)
				{
					ippiCompareC_16u_C1R(pDst[i], dstStep, maxValue, mask, maskStep,roi, ippCmpGreater);
					ippiSet_16u_C1MR(maxValue, pDst[i], dstStep, roi, mask, maskStep);
				}
				status = ippiCopy_16u_P3C3R(pDst, dstStep, buffer, step, roi);
				for(int i = 0; i < 3; i++)
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

vector<vector<Point>> findContours16U(unsigned short* buffer, int width, int height)
{
	Mat source(height, width, CV_16UC1, buffer);
	Mat binary = Mat::zeros(source.size(), CV_8UC1);
	source.convertTo(binary, CV_8UC1);
	vector<vector<Point>> contours;
	findContours(binary, contours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_NONE );	
	return contours;
}
