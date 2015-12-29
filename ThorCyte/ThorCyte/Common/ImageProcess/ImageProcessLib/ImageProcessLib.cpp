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

IPP_LIB_API int fnipp_lib_resize_16uC1(unsigned short* srcBuffer, int srcWidth, int srcHeight, 
								 unsigned short* dstBuffer, int dstWidth, int dstHeight)
{
	IppiSize size = {srcWidth, srcHeight};
	IppiRect srect = {0, 0, srcWidth, srcHeight};
	IppiRect drect = {0, 0, dstWidth, dstHeight};
	Ipp16u* tempBuffer = ippsMalloc_16u(dstWidth*dstHeight);
	memset(tempBuffer,0, sizeof(Ipp16u)*dstWidth*dstHeight);
	Ipp8u *buf;
	int bufsize;
	IppStatus status = ippStsNoErr;
	double xFactor = (double)dstWidth/(double)srcWidth;
	double yFactor = (double)dstHeight/(double)srcHeight;
	/* calculation of work buffer size */
	ippiResizeGetBufSize( srect, drect, 1, IPPI_INTER_LINEAR, &bufsize );
	#ifdef _OPENMP
	/* parallel algorithm */
	#pragma omp parallel
	{
	#pragma omp master
		{
			/* number of threads */
			nt = omp_get_num_threads();
			/* memory allocate */
			buf = ippsMalloc_8u( nt*bufsize );
		}
	#pragma omp barrier
		{
			/* thread's number */
			int id = omp_get_thread_num();
			/* ROI for one slice */
			drect.y = (drect.height/nt)*id;
			drect.height = (drect.height/nt)*(id+1);
			if( NULL != buf )
			status = ippiResizeSqrPixel_32f_C1R(
			src, size, 12*sizeof(Ipp32f), srect,
			dst, 6*sizeof(Ipp32f), drect,0.5, 0.5, 0, 0, IPPI_INTER_SUPER, buf+id*bufsize );
		}
	}
	#else
	/* single algorithm */
		{
			/* memory allocate */
			buf = ippsMalloc_8u( bufsize );
			if( NULL != buf )
				status = ippiResizeSqrPixel_16u_C1R(srcBuffer, size, srcWidth*sizeof(Ipp16u), srect,tempBuffer, 
				dstWidth*sizeof(Ipp16u), drect, xFactor, yFactor, 0, 0, IPPI_INTER_LINEAR, buf );
		}
	#endif

	//MoveBuffer_16u(tempBuffer,  dstBuffer, dstWidth, dstHeight);
	memcpy(dstBuffer, tempBuffer, dstHeight*dstWidth*sizeof(unsigned short));
	/* memory free */
	if( NULL != buf ) ippsFree( buf );
	if( NULL != tempBuffer ) ippsFree( tempBuffer );
	return status;
}

IPP_LIB_API int fnipp_lib_AddConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer)
{
	int elementSize = sizeof(unsigned short);
	int srcStep = width*elementSize;
	IppiSize roi = {width, height};
	IppStatus status = ippiAddC_16u_C1RSfs(srcBuffer, width*elementSize, value,dstBuffer, width*elementSize, roi, 0);
	return status;
}

IPP_LIB_API int fnipp_lib_SubConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer)
{
	int elementSize = sizeof(unsigned short);
	int srcStep = width*elementSize;
	IppiSize roi = {width, height};
	IppStatus status = ippiSubC_16u_C1RSfs(srcBuffer, width*elementSize, value,dstBuffer, width*elementSize, roi, 0);
	return status;
}

IPP_LIB_API int fnipp_lib_MulConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer)
{
	int elementSize = sizeof(unsigned short);
	int srcStep = width*elementSize;
	IppiSize roi = {width, height};
	IppStatus status = ippiMulC_16u_C1RSfs(srcBuffer, width*elementSize, value, dstBuffer, width*elementSize, roi, 0);
	return status;
}


void MoveBuffer_16u(unsigned short* srcBuffer, unsigned short* dstBuffer, int dstWidth, int dstHeight)
{
	const int size  = sizeof(unsigned short);
	for(int i = 0; i < dstHeight; i++)
	{
		memcpy(dstBuffer+dstWidth*i, srcBuffer+dstWidth*i, dstWidth*size);
	}
}