// ImageProcessLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ImageProcessLib.h"

LIBCV_API int fnipp_lib_init(void)
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

LIBCV_API Ipp64u fnipp_lib_getCpuClocks()
{
	return ippGetCpuClocks();
}

LIBCV_API int fnipp_lib_resize_16u(unsigned short* srcBuffer, int srcWidth, int srcHeight,
									 unsigned short* dstBuffer, int dstWidth, int dstHeight, int channels)
{
	const IppiSize size = {srcWidth, srcHeight};
	const IppiRect srect = {0, 0, srcWidth, srcHeight};
	const IppiRect drect = {0, 0, dstWidth, dstHeight};
	const int srcStep = srcWidth * channels * sizeof(Ipp16u);
	const int dstStep = dstWidth * channels * sizeof(Ipp16u);
	Ipp8u *buf;
	int bufsize = 0;
	IppStatus status = ippStsNoErr;
	const double xFactor = static_cast<double>(dstWidth)/static_cast<double>(srcWidth);
	const double yFactor = static_cast<double>(dstHeight)/static_cast<double>(srcHeight);
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



LIBCV_API int fnipp_lib_addConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status;
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
			break;
		}
		default:
		status = ippStsAacChanErr;
		break;
	}
	if(status==ippStsNoErr)
		status = saturate(dstBuffer, width, height, channels, maxValue);
	return status;
}


LIBCV_API int fnipp_lib_add_16u(unsigned short* srcBuffer1, unsigned short* srcBuffer2, 
								  int width, int height, unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status;
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

LIBCV_API int fnipp_lib_add_16uI(Ipp16u* srcBuffer, int width, int height, int channels, 
								   unsigned short maxValue, unsigned short* srcDstBuffer)
{
	const int scaleFactor = 0;
	const int step = width * channels * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippStsNoErr;
	switch (channels)
	{		
		case 1:
		{
			status = ippiAdd_16u_C1IRSfs(srcBuffer, step, srcDstBuffer, step,  roi, scaleFactor);
			break;
		}
		case 3:
		{
			status = status = ippiAdd_16u_C3IRSfs(srcBuffer, step, srcDstBuffer, step,  roi, scaleFactor);
			break;
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	if(status == ippStsNoErr)
		status = saturate(srcDstBuffer, width, height, channels, maxValue);

	return status;
}

LIBCV_API int fnipp_lib_subConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status;
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
			break;
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	
	return status;
}

LIBCV_API int fnipp_lib_sub_16u(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
									int width, int height, unsigned short* dstBuffer, int channels)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status;
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

LIBCV_API int fnipp_lib_sub_16uI(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
								   int width, int height, int channels)
{
	const int scaleFactor = 0;
	const int step = width * channels * sizeof(Ipp16u);
	const IppiSize roi = {width, height};
	IppStatus status;
	switch (channels)
	{
		case 1:
		{
			status = ippiSub_16u_C1IRSfs(subtracterBuffer, step, minuendBuffer, step, roi, scaleFactor);
			break;
		}
		case 3:
		{
			status = ippiSub_16u_C3IRSfs(subtracterBuffer, step, minuendBuffer, step,  roi, scaleFactor);
			break;
		}
		default:
		status = ippStsChannelErr;
		break;
	}
	return status;
}

LIBCV_API int fnipp_lib_mulConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, int height, 
										  unsigned short* dstBuffer, int channels, unsigned short maxValue)
{
	int elementSize = sizeof(unsigned short);
	int srcStep = width * elementSize * channels;
	const IppiSize roi = {width, height};
	IppStatus status;
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

LIBCV_API int fnipp_lib_max_16u(const unsigned short* buffer, int width, int height,
								  int channels, unsigned short* pMaxValue)
{
	const IppiSize roi = {width, height};
	IppStatus status;
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



LIBCV_API int fnipp_lib_min_16u(unsigned short* buffer, int width, int height, 
								  int channels, unsigned short* pMinValue)
{
	const IppiSize roi = {width, height};
	IppStatus status;
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

LIBCV_API int fnipp_lib_maxEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	IppStatus status;
	const int step = width * channels * ElementSize;
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

LIBCV_API int fnipp_lib_minEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	IppStatus status;
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

LIBCV_API int fnipp_lib_invert_16u(unsigned short* srcBuffer, int width, int height, 
									 int channels, unsigned short maxValue, unsigned short* dstBuffer)
{
	const int step = width * channels * ElementSize;
	const int scaleFactor = 0;
	const IppiSize roi = {width, height};
	int minuendBufferStep = 0;
	IppStatus status;
	switch (channels)
	{
		case 1:
		{
			Ipp16u* minuendBuffer = ippiMalloc_16u_C1(width, height, &minuendBufferStep);
			status = ippiSet_16u_C1R(maxValue, minuendBuffer, minuendBufferStep, roi);
			if(status == ippStsNoErr) 
				status = ippiSub_16u_C1RSfs(srcBuffer, step, minuendBuffer, minuendBufferStep, dstBuffer, step, roi, scaleFactor);				
			ippiFree(minuendBuffer);
			break;
		}
		case 3:
		{
			Ipp16u* minuendBuffer = ippiMalloc_16u_C3(width, height, &minuendBufferStep);
			Ipp16u maxValueArray[3] = {maxValue, maxValue, maxValue};
			status = ippiSet_16u_C3R(maxValueArray, minuendBuffer, minuendBufferStep, roi);
			if(status == ippStsNoErr)
				status = ippiSub_16u_C3RSfs(srcBuffer, step, minuendBuffer, minuendBufferStep, dstBuffer, step, roi,scaleFactor);
			ippiFree(minuendBuffer);
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

LIBCV_API int fnipp_lib_invert_16uI(unsigned short* srcDstBuffer, int width, int height, 
									 int channels, unsigned short maxValue)
{
	const int step = width * channels * ElementSize;
	const int scaleFactor = 0;
	const IppiSize roi = {width, height};
	unsigned short* subtracterBuffer = static_cast<unsigned short*>(malloc(step * height));
	memcpy(subtracterBuffer, srcDstBuffer, step * height);
	IppStatus status;
	switch (channels)
	{
		case 1:
		{
			
			status = ippiSet_16u_C1R(maxValue, srcDstBuffer, step, roi);
			if(status == ippStsNoErr) 
				status = ippiSub_16u_C1IRSfs(subtracterBuffer, step, srcDstBuffer, step, roi, scaleFactor);				
			break;
		}
		case 3:
		{
			Ipp16u maxValueArray[3] = {maxValue, maxValue, maxValue};
			status = ippiSet_16u_C3R(maxValueArray, srcDstBuffer, step, roi);
			if(status == ippStsNoErr)
				status = ippiSub_16u_C3IRSfs(subtracterBuffer, step, srcDstBuffer, step, roi,scaleFactor);
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
	}
	free(subtracterBuffer);
	return status;

}



LIBCV_API int fnipp_lib_threshold_16uC1(const unsigned short* srcBuffer, int width, int height, 
									unsigned short threshold, unsigned short* dstBuffer)
{
	const IppiSize roi = {width, height};
	const int step = width * sizeof(unsigned short);
	const Ipp16u valueLT = 0;
	const Ipp16u valueGT = 0x3fff;
	int maskStep = 0;
	Ipp8u* mask = ippiMalloc_8u_C1(width, height, &maskStep);
	IppStatus status = ippiThreshold_LTValGTVal_16u_C1R(srcBuffer, step, dstBuffer, step, roi,threshold,valueLT, threshold, valueGT);
	ippiCompareC_16u_C1R (dstBuffer, step, threshold, mask, maskStep, roi, ippCmpEq);
	ippiSet_16u_C1MR(valueGT, dstBuffer, step,roi, mask, maskStep);
	ippiFree(mask);
	return status;
}

LIBCV_API int fnipp_lib_otsuThreshold_16uC1(const unsigned short* srcBuffer, int width, int height, unsigned short* dstBuffer)
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
			status = static_cast<IppStatus>(fnipp_lib_threshold_16uC1(srcBuffer, width, height, 
			                                                          unsigned short(threshold<<8), dstBuffer));
			ippiFree(srcBuffer8u);
			return status;
		}
	}
}

LIBCV_API int fnipp_lib_dilate_16uC1(unsigned short* srcBuffer, int width,
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
	IppStatus status; 
	ippiCopyConstBorder_16u_C1R(srcBuffer, step, roi, innerSrcBuffer, 
												srcStep,innerRoi, offsetTop, offsetLeft, 0); 
	status = ippiDilate_16u_C1R(&innerSrcBuffer[srcElementPerLine*offsetTop+offsetLeft], srcStep, 
		dstBuffer, step, roi, mask, ippMaskSize, anchor);
	ippiFree(innerSrcBuffer);
	return status;
}


LIBCV_API int fnipp_lib_sum_16uC1M(unsigned short* buffer, int width, int height, unsigned char* mask, int maskStep, double* sum)
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



LIBCV_API int fnipp_lib_filter_16u(unsigned short* srcBuffer, int width, int height, int channels, 
									 unsigned short* dstBuffer, FilterType type, int maskSize, unsigned short maxValue)
{
	string path = "C:\\Users\\root\\Desktop\\Filter output\\Intel IPP\\in c time cost\\";
    string filename = get_filter_name(type, maskSize) + ".txt";
	path = path + filename;
	LARGE_INTEGER f;
	QueryPerformanceFrequency(&f);
	LARGE_INTEGER start, end;
	QueryPerformanceCounter(&start);
	const Ipp32s* pKernel = get_filter_kernel(type, maskSize);
	const IppiSize roi = {width, height};
	IppStatus status;
	const int elementSize = sizeof(unsigned short);
	const int step = width * elementSize * channels;
	const IppiSize kernelSize = {maskSize, maskSize};	
	const int divisor = 1;
	int stepBorder = 0;
	IppiPoint anchor = {0, 0};
	Ipp16u* srcWithBorder = get_border(srcBuffer, width, height, channels, maskSize, &stepBorder, &anchor);
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
			break;
		}
		default:
		{
			status = ippStsChannelErr;
			break;
		}
		
	}
	ippiFree(srcWithBorder);
	saturate(dstBuffer, width, height, channels, maxValue);
	QueryPerformanceCounter(&end);
	double ms = (end.QuadPart - start.QuadPart)*1000.0/f.QuadPart;
	FILE* file = nullptr;
	if(fopen_s(&file, path.c_str(),"a+")==0)
	{
		fprintf_s(file, "%0.1f\n",ms);
		fclose(file);
	}
	return status;
}


LIBCV_API int fnipp_lib_mean_16uC1(const unsigned short* srcBuffer, int width, int height, double* pAverage)
{
	const Ipp32s step = ElementSize * width;
	const IppiSize roi = {width, height};
	IppStatus status = ippiMean_16u_C1R(srcBuffer, step, roi, pAverage);	
	return status;
}

LIBCV_API int fnipp_lib_And_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, 
								  int width, int height, unsigned short* dstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiAnd_16u_C1R(srcBuffer1, step, srcBuffer2, step, dstBuffer, step, roi);	
	return status;
}

LIBCV_API int fnipp_lib_And_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiAnd_16u_C1IR(srcBuffer, step, srcDstBuffer, step, roi);	
	return status;
}

LIBCV_API int fnipp_lib_Or_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, int width, 
								   int height, unsigned short* dstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiOr_16u_C1R(srcBuffer1, step, srcBuffer2, step, dstBuffer, step, roi);	
	return status;
}

LIBCV_API int fnipp_lib_Or_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiOr_16u_C1IR(srcBuffer, step, srcDstBuffer, step, roi);	
	return status;
}

LIBCV_API int fnipp_lib_Xor_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, int width, 
								    int height, unsigned short* dstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiXor_16u_C1R(srcBuffer1, step, srcBuffer2, step, dstBuffer, step, roi);	
	return status;
}

LIBCV_API int fnipp_lib_Xor_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer)
{
	const int step = width * ElementSize;
	const IppiSize roi = {width, height};
	IppStatus status = ippiXor_16u_C1IR(srcBuffer, step, srcDstBuffer, step, roi);	
	return status;
}


LIBCV_API int fnipp_lib_rotateShift_16u(const unsigned short* srcBuffer, int width, int height, int channels, double angle,
										   int shiftX, int shiftY, unsigned short* dstBuffer)
{
	const int step = width * channels * ElementSize;
	const IppiRect roi = {0, 0, width, height};
	const IppiSize imageSize = {width, height};
	double offsetX = 0;
	double offsetY = 0;
	ippiGetRotateShift(width/2, height/2,angle, &offsetX, &offsetY);
	IppStatus status;
	switch (channels)
	{
		case 1:
		{
			status = ippiRotate_16u_C1R(srcBuffer, imageSize, step, roi, dstBuffer, step, roi, 
				angle, shiftX+offsetX, shiftY+offsetY, IPPI_INTER_LINEAR );
			break;
		}
		case 3:
		{
			status = ippiRotate_16u_C1R(srcBuffer, imageSize, step, roi, dstBuffer, step, roi, 
			angle, shiftX+offsetX, shiftY+offsetY, IPPI_INTER_LINEAR );
			break;
		}

		default:
			status = ippStsChannelErr;
			break;
	}
	return status;

}


LIBCV_API int fncv_lib_findContours_16uC1(unsigned short* srcBuffer, int width, int height, 
										  int** ppBlobs, int** ppPointsCountPerBlob)
{
	list<vector<POINT>> contours = find_contours16U(srcBuffer,width, height);
	size_t blobCount = contours.size();
	if(blobCount == 0) return 0;
	*ppPointsCountPerBlob = static_cast<int*> (malloc(sizeof(int) * blobCount));
	size_t totalPointsCount = 0;
	for(auto it = contours.begin(); it!=contours.end();++it)
	{
		totalPointsCount+=(*it).size();
	}
	*ppBlobs = static_cast<int*> (malloc(sizeof(int) * totalPointsCount * 2));
	int index = 0;
	int count = 0;
	for(auto iter = contours.begin(); iter!=contours.end(); ++iter, count++)
	{
		vector<POINT> contour = *iter;
		size_t n = contour.size();
		(*ppPointsCountPerBlob)[count] = static_cast<int>(n);
		for(size_t j=0; j<n; j++)
		{
			POINT point = contour[j];
			(*ppBlobs)[index] = point.x;
			(*ppBlobs)[index+1] = point.y;
			index += 2;
		}
	}
	return static_cast<int>(blobCount);
}

LIBCV_API void free_buffer(void* pBuffer)
{
	free(pBuffer);
}