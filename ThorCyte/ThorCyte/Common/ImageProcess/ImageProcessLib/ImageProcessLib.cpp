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
	Ipp8u* srcBuffer8u = ippsMalloc_8u(width * height);
	const int srcStep = width * height * sizeof(unsigned short);
	const int src8uStep = width * height;
	const IppiSize roi = {width, height};
	IppStatus status = ippiConvert_16u8u_C1R(srcBuffer, srcStep, srcBuffer8u, src8uStep, roi);
	Ipp8u threshold = 0;
	if(status != ippStsNoErr) 
	{
		ippFree(srcBuffer8u);
		return status;
	}
	else
	{
		status = ippiComputeThreshold_Otsu_8u_C1R(srcBuffer8u, src8uStep, roi, &threshold);
		if(status != ippStsNoErr)
		{
			ippFree(srcBuffer8u);
			return status;
		}
		else
		{
			status = (IppStatus)fnipp_lib_threshold_16uC1(srcBuffer, width, height, unsigned short(threshold<<8), dstBuffer);
			ippFree(srcBuffer8u);
			return status;
		}
	}
}

IppStatus saturate(unsigned short* buffer, int width, int height, int channels, unsigned short maxValue)
{
	const IppiSize roi = {width, height};
	const int step = width * channels * sizeof(unsigned short);
	const int maskStep  = width;
	Ipp8u* mask = ippsMalloc_8u(width * height);
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
			/*Ipp16u maxValueArray[3] = {maxValue, maxValue, maxValue};
			status = ippiCompareC_16u_C3R(buffer, step, maxValueArray, mask, maskStep,roi, ippCmpGreater);
			if(status == ippStsNoErr)
				status = ippiSet_16u_C3MR(maxValueArray, buffer, step, roi, mask, maskStep);
			else
				return status;
			break;*/
			Ipp16u* pDst[3];
			const int dstStep = width * sizeof(unsigned short);
			pDst[0] = ippsMalloc_16u(width * height);
			pDst[1] = ippsMalloc_16u(width * height);
			pDst[2] = ippsMalloc_16u(width * height);
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
	ippFree(mask);
	
	return status;
}