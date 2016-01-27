// ImageProcessLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ImageProcessLib.h"

IPP_LIB_API int fnipp_lib_init(void)
{
	IppStatus status;
	/* Init IPP library */
	status = ippStaticInit();
	if (ippStsNoErr != status) 
	{
		printf("--- Features error of ippStaticInit %d ---\n", status);
	}
	return 0;
}

IPP_LIB_API unsigned __int64 fnipp_lib_getCpuClocks()
{
	return ippGetCpuClocks();
}

IPP_LIB_API int fnipp_lib_resize_16u(unsigned short* srcBuffer, int srcWidth, int srcHeight,
									 unsigned short* dstBuffer, int dstWidth, int dstHeight, int channels)
{
	const IppiSize size = {srcWidth, srcHeight};
	const IppiRect srect = {0, 0, srcWidth, srcHeight};
	const IppiRect drect = {0, 0, dstWidth, dstHeight};
	const int srcStep = srcWidth * channels * sizeof(Ipp16u);
	const int dstStep = dstWidth * channels * sizeof(Ipp16u);
	Ipp8u *buf = NULL;
	int bufsize = 0;
	IppStatus status = ippStsNoErr;
	const double xFactor = (double)dstWidth/(double)srcWidth;
	const double yFactor = (double)dstHeight/(double)srcHeight;
	/* calculation of work buffer size */
	ippiResizeGetBufSize( srect, drect, channels, IPPI_INTER_LINEAR, &bufsize );
	buf = ippsMalloc_8u( bufsize );
	if( NULL != buf )
	{
		switch (channels)
		{
			case 1:
			{
				status = ippiResizeSqrPixel_16u_C1R(srcBuffer, size, srcStep, srect, dstBuffer, 
													dstStep, drect, xFactor, yFactor, 0, 0, IPPI_INTER_LINEAR, buf );
				break;
			}
			case 3:
			{
				status = ippiResizeSqrPixel_16u_C3R(srcBuffer, size, srcStep, srect, dstBuffer, 
													dstStep, drect, xFactor, yFactor, 0, 0, IPPI_INTER_LINEAR, buf );
				break;
			}
			default:
			status = ippStsChannelErr;
			break;
		}
		
	}
	if( NULL != buf ) ippsFree( buf );
	return status;
}



IPP_LIB_API int fnipp_lib_addConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int len = width * height * channels;
	switch (channels)
	{
		case 1:
		{
			status = ippiAddC_16u_C1RSfs(srcBuffer, step, value, dstBuffer, step, roi, scaleFactor);
					
			break;
		}
		case 3:
		{
			Ipp16u vector[3] = {value, value, value}; 
			status = ippiAddC_16u_C3RSfs(srcBuffer, step, vector, dstBuffer, step, roi, scaleFactor);
		}
		default:
		status = ippStsAacChanErr;
		break;
	}
	if(status==ippStsNoErr)
		status = saturate(dstBuffer, width, height, channels, maxValue);
	return status;
}


IPP_LIB_API int fnipp_lib_add_16u(unsigned short* srcBuffer1, unsigned short* srcBuffer2, 
								  int width, int height, unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int len = width * height * channels;
	switch (channels)
	{		
		case 1:
		{
			status = ippiAdd_16u_C1RSfs(srcBuffer1, step, srcBuffer2, step, dstBuffer, step, roi, scaleFactor);
			break;
		}
		case 3:
		{
			status = ippiAdd_16u_C3RSfs(srcBuffer1, step, srcBuffer2, step, dstBuffer, step, roi, scaleFactor);
			break;
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	if(status == ippStsNoErr)
		status = saturate(dstBuffer, width, height, channels, maxValue);

	return status;
}

IPP_LIB_API int fnipp_lib_subConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	switch (channels)
	{
		case 1:
		{
			status = ippiSubC_16u_C1RSfs(srcBuffer, step, value, dstBuffer, step, roi, scaleFactor);
			break;
		}
		case 3:
		{
			Ipp16u vector[3] = {value, value, value}; 
			status = ippiSubC_16u_C3RSfs(srcBuffer, step, vector, dstBuffer, step, roi, scaleFactor);
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	
	return status;
}

IPP_LIB_API int fnipp_lib_sub_16u(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
									int width, int height, unsigned short* dstBuffer, int channels)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	switch (channels)
	{
		case 1:
		{
			status = ippiSub_16u_C1RSfs(subtracterBuffer, step, minuendBuffer, step, dstBuffer, step, roi, scaleFactor);
			break;
		}
		case 3:
		{
			status = ippiSub_16u_C3RSfs(subtracterBuffer, step, minuendBuffer, step, dstBuffer, step, roi, scaleFactor);
			break;
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	return status;
}


IPP_LIB_API int fnipp_lib_mulConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, int height, 
										  unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	int elementSize = sizeof(unsigned short);
	int srcStep = width * elementSize * channels;
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int scaleFactor = 0;
	switch(channels)
	{
		case 1:
		{
			status = ippiMulC_16u_C1RSfs(srcBuffer, srcStep, value, dstBuffer, srcStep, roi, scaleFactor);
			break;
		}
		case 3:
		{
			Ipp16u values[3] = {value, value, value};
			status = ippiMulC_16u_C3RSfs(srcBuffer, srcStep, values, dstBuffer, srcStep, roi, scaleFactor);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}

	}
	if(status == ippStsNoErr)
		status = saturate(dstBuffer, width, height, channels, maxValue);
	return status;
}

IPP_LIB_API int fnipp_lib_max_16u(const unsigned short* buffer, int width, int height,
								  int channels, unsigned short* pMaxValue)
{
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int step = width * channels * sizeof(Ipp16u);
	switch (channels)
	{
		case 1:
		{
			status = ippiMax_16u_C1R(buffer, step, roi, pMaxValue);
			break;
		}
		case 3:
		{
			status=ippiMax_16u_C3R(buffer, step, roi, pMaxValue);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	return status;
}



IPP_LIB_API int fnipp_lib_min_16u(unsigned short* buffer, int width, int height, 
								  int channels, unsigned short* pMinValue)
{
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int step = width * channels * sizeof(Ipp16u);
	switch (channels)
	{
		case 1:
		{
			status = ippiMin_16u_C1R(buffer, step, roi, pMinValue);
			break;
		}
		case 3:
		{
			status=ippiMin_16u_C3R(buffer, step, roi, pMinValue);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	return status;
}

IPP_LIB_API int fnipp_lib_maxEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int step = width * channels * sizeof(Ipp16u);
	memcpy(dstBuffer, secondBuffer, step * height);
	switch (channels)
	{
		case 1:
		{
			status  = ippiMaxEvery_16u_C1IR(firstBuffer, step, dstBuffer, step, roi);
			break;
		}
		case 3:
		{
			status=ippiMaxEvery_16u_C3IR(firstBuffer, step, dstBuffer, step, roi);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	return status;
}

IPP_LIB_API int fnipp_lib_minEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int step = width * channels * sizeof(Ipp16u);
	memcpy(dstBuffer, secondBuffer, step * height);
	switch (channels)
	{
		case 1:
		{
			status  = ippiMinEvery_16u_C1IR(firstBuffer, step, dstBuffer, step, roi);
			break;
		}
		case 3:
		{
			status=ippiMinEvery_16u_C3IR(firstBuffer, step, dstBuffer, step, roi);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	return status;
}

IPP_LIB_API int fnipp_lib_invert_16u(unsigned short* srcBuffer, int width, int height, 
									 int channels, unsigned short maxValue, unsigned short* dstBuffer)
{
	const int step = width * channels * sizeof(unsigned short);
	const int scaleFactor = 0;
	const int size = step * height;
	unsigned short* minuendBuffer = (unsigned short*)malloc(size);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	switch (channels)
	{
		case 1:
		{
			status = ippiSet_16u_C1R(maxValue, minuendBuffer, step, roi);
			if(status == ippStsNoErr) 
				status = ippiSub_16u_C1RSfs(srcBuffer, step, minuendBuffer, step, dstBuffer, step, roi, scaleFactor);				
			else
				return status;
			break;
		}
		case 3:
		{
			Ipp16u maxValueArray[3] = {maxValue, maxValue, maxValue};
			status = ippiSet_16u_C3R(maxValueArray, minuendBuffer, step, roi);
			if(status == ippStsNoErr)
				status = ippiSub_16u_C3RSfs(minuendBuffer, step, srcBuffer, step, dstBuffer, step, roi,scaleFactor);
			else
				return status;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	free(minuendBuffer);
	return status;
	
}



IPP_LIB_API int fnipp_lib_threshold_16uC1(unsigned short* srcBuffer, int width, int height, 
									unsigned short threshold, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	const int step = width * sizeof(unsigned short);
	IppStatus status = ippiThreshold_LTVal_16u_C1R(srcBuffer, step, dstBuffer, step, roi, threshold, 0);
	return status;
}

IPP_LIB_API int fnipp_lib_otsuThreshold_16uC1(unsigned short* srcBuffer, int width, int height, unsigned short* dstBuffer)
{
	int src8uStep = 0;
	Ipp8u* srcBuffer8u = ippiMalloc_8u_C1(width , height, & src8uStep);
	const int srcStep = width * sizeof(unsigned short);

	const IppiSize roi = {width, height};
	IppStatus status = ippiConvert_16u8u_C1R(srcBuffer, srcStep, srcBuffer8u, src8uStep, roi);
	Ipp8u threshold = 0;
	if(status != ippStsNoErr) 
	{
		ippiFree(srcBuffer8u);
		return status;
	}
	else
	{
		status = ippiComputeThreshold_Otsu_8u_C1R(srcBuffer8u, src8uStep, roi, &threshold);
		if(status != ippStsNoErr)
		{
			ippiFree(srcBuffer8u);
			return status;
		}
		else
		{
			status = (IppStatus)fnipp_lib_threshold_16uC1(srcBuffer, width, height, 
														unsigned short(threshold<<8), dstBuffer);
			ippiFree(srcBuffer8u);
			return status;
		}
	}
}

IPP_LIB_API int fnipp_lib_dilate_16uC1(unsigned short* srcBuffer, int width,
									   int height, int maskSize, unsigned short* dstBuffer)
{
	const int anchorX = maskSize/2;
	const int anchorY = anchorX;
	const int offsetTop = anchorY;
	const int offsetLeft = anchorX;
	const int offsetRight = maskSize - anchorX - 1;
	const int offsetBottom = maskSize - anchorY - 1;
	const int elementSize = sizeof(unsigned short);
	const IppiPoint anchor = {anchorX, anchorY};
	const IppiSize ippMaskSize = {maskSize, maskSize};
	const IppiSize innerRoi = {width + offsetLeft + offsetRight, height + offsetTop + offsetBottom};
	const IppiSize roi = {width, height};	
	const int step = width * elementSize;
	int srcStep = 0;
	int maskStep = 0;
	Ipp16u* innerSrcBuffer = ippiMalloc_16u_C1(innerRoi.width , innerRoi.height, &srcStep);	
	Ipp8u* mask = ippiMalloc_8u_C1(maskSize,  maskSize, &maskStep);
	memset(mask,1, maskStep * maskSize);
	const int srcElementPerLine = srcStep / elementSize;
	IppStatus status = ippiCopyConstBorder_16u_C1R(srcBuffer, step, roi, innerSrcBuffer, 
												srcStep,innerRoi, offsetTop, offsetLeft, 0); 
	status = ippiDilate_16u_C1R(&innerSrcBuffer[srcElementPerLine*offsetTop+offsetLeft], srcStep, 
		dstBuffer, step, roi, mask, ippMaskSize, anchor);
	ippiFree(innerSrcBuffer);
	return status;
}


IPP_LIB_API int fnipp_lib_sum_16uC1M(unsigned short* buffer, int width, int height, unsigned char* mask, int maskStep, double* sum)
{
	int maskedBufferStep = 0;
	Ipp16u* maskedBuffer = ippiMalloc_16u_C1(width, height, &maskedBufferStep);
	const int srcStep = width * sizeof(unsigned short);
	memset(maskedBuffer, 0, maskedBufferStep * height);
	const IppiSize roi = {width, height};
	IppStatus status = ippiCopy_16u_C1MR(buffer,srcStep, maskedBuffer, maskedBufferStep, roi, mask, maskStep);
	status = ippiSum_16u_C1R(maskedBuffer, maskedBufferStep, roi, sum);
	return status;
}



IPP_LIB_API int fnipp_lib_filter_16u(unsigned short* srcBuffer, int width, int height, int channels, 
									 unsigned short* dstBuffer, FilterType type, int maskSize)
{
	const Ipp32s* pKernel = getFilterKernel(type, maskSize);
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	const int elementSize = sizeof(unsigned short);
	const int step = width * elementSize * channels;
	const IppiSize kernelSize = {maskSize, maskSize};	
	const int divisor = 1;
	int stepBorder = 0;
	IppiPoint anchor = {0, 0};
	Ipp16u* srcWithBorder = getBorder(srcBuffer, width, height, channels, maskSize, &stepBorder, &anchor);
	const int srcElementPerLine = stepBorder / elementSize;
	switch (channels)
	{
		case 1:
		{
			status = ippiFilter_16u_C1R(&srcWithBorder[srcElementPerLine*anchor.y+anchor.x*channels], 
				stepBorder, dstBuffer, step, roi, pKernel, kernelSize, anchor, divisor);
			break;
		}
		case 3:
		{
			status = ippiFilter_16u_C3R(&srcWithBorder[srcElementPerLine*anchor.y+anchor.x*channels], 
				stepBorder, dstBuffer, step, roi, pKernel, kernelSize, anchor, divisor);
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
		
	}
	ippiFree(srcWithBorder);
	saturate(dstBuffer, width, height, channels, MaxValue);
	return status;
}


IPP_LIB_API int fnipp_lib_mean_16uC1(const Ipp16u* srcBuffer, Ipp32s width, Ipp32s height, Ipp64f* pAverage)
{
	const Ipp32s step = ElementSize * width;
	const IppiSize roi = {width, height};
	IppStatus status = ippiMean_16u_C1R(srcBuffer, step, roi, pAverage);	
	return status;
}




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
	return NULL;
	
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
	const IppiPoint anchor = {anchorX, anchorY};
	const IppiSize ippMaskSize = {maskSize, maskSize};
	const IppiSize roiWithBorder = {width + offsetLeft + offsetRight, height + offsetTop + offsetBottom};
	const IppiSize roi = {width, height};	
	const int srcStep = width * elementSize;
	int stepWithBorder = 0;		
	Ipp16u* bufferWithBorder = ippiMalloc_16u_C1(roiWithBorder.width , roiWithBorder.height, &stepWithBorder);	
	IppStatus status = ippiCopyConstBorder_16u_C1R(buffer, srcStep, roi, bufferWithBorder, 
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
	IppStatus status = ippStsNoErr;
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
					status = ippiCompareC_16u_C1R(pDst[i], dstStep, maxValue, mask, maskStep,roi, ippCmpGreater);
					status = ippiSet_16u_C1MR(maxValue, pDst[i], dstStep, roi, mask, maskStep);
				}
				status = ippiCopy_16u_P3C3R(pDst, dstStep, buffer, step, roi);
				for(int i = 0; i < 3; i++)
					ippiFree(pDst[i]);
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