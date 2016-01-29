#pragma once
#include "stdafx.h"
#include "Utils.h"
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the IPP_LIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// IPP_LIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef IPP_LIB_EXPORTS
#define IPP_LIB_API extern "C" __declspec(dllexport)
#else
#define IPP_LIB_API extern "C" __declspec(dllimport)
#endif


IPP_LIB_API int fnipp_lib_init(void);
IPP_LIB_API Ipp64u fnipp_lib_getCpuClocks();
IPP_LIB_API int fnipp_lib_resize_16u(unsigned short* srcBuffer, int srcWidth, int srcHeight,
									 unsigned short* dstBuffer, int dstWidth, int dstHeight, int channels);
IPP_LIB_API int fnipp_lib_addConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels, unsigned short maxValue);

IPP_LIB_API int fnipp_lib_add_16u(unsigned short* srcBuffer1, unsigned short* srcBuffer2, 
								  int width, int height, unsigned short* dstBuffer, int channels, unsigned short maxValue);

IPP_LIB_API int fnipp_lib_add_16uI(Ipp16u* srcBuffer, int width, int height, int channels, 
								   unsigned short maxValue, unsigned short* srcDstBuffer);
IPP_LIB_API int fnipp_lib_subConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels);

IPP_LIB_API int fnipp_lib_sub_16u(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
									int width, int height, unsigned short* dstBuffer, int channels);
IPP_LIB_API int fnipp_lib_sub_16uI(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
								   int width, int height, int channels);

IPP_LIB_API int fnipp_lib_mulConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, int height,
										  unsigned short* dstBuffer, int channels, unsigned short maxValue);
IPP_LIB_API int fnipp_lib_max_16u(const unsigned short* buffer, int width, int height, int channels, unsigned short* pMaxValue);

IPP_LIB_API int fnipp_lib_min_16u(unsigned short* buffer, int width, int height, int channels, unsigned short* pMinValue);

IPP_LIB_API int fnipp_lib_maxEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_minEvery_16u(unsigned short* firstBuffer, unsigned short* secondBuffer, int width, 
									   int height, int channels, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_invert_16u(unsigned short* srcBuffer, int width, int height, 
									 int channels, unsigned short maxValue, unsigned short* dstBuffer);
IPP_LIB_API int fnipp_lib_invert_16uI(unsigned short* srcDstBuffer, int width, int height, 
									  int channels, unsigned short maxValue);
IPP_LIB_API int fnipp_lib_threshold_16uC1(unsigned short* srcBuffer, int width, int height, 
									unsigned short threshold, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_otsuThreshold_16uC1(unsigned short* srcBuffer, int width, int height, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_dilate_16uC1(unsigned short* srcBuffer, int width, int height, int maskSize, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_sum_16uC1M(unsigned short* buffer, int width, int height, unsigned char* mask, int maskStep,  double* sum);

IPP_LIB_API int fnipp_lib_filter_16u(unsigned short* srcBuffer, int width, int height, int channels, unsigned short* dstBuffer, 
									 FilterType type, int maskSize, unsigned short maxValue);

IPP_LIB_API int fnipp_lib_mean_16uC1(const unsigned short* srcBuffer, int width, int height, double* pAverage);

IPP_LIB_API int fnipp_lib_And_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, int width, 
								    int height, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_And_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer);

IPP_LIB_API int fnipp_lib_Or_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, int width, 
								   int height, unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_Or_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer);

IPP_LIB_API int fnipp_lib_Xor_16uC1(const unsigned short* srcBuffer1, const unsigned short* srcBuffer2, int width, 
								    int height, unsigned short* dstBuffer);
IPP_LIB_API int fnipp_lib_Xor_16uC1I(const unsigned short* srcBuffer, int width, int height, unsigned short* srcDstBuffer);

IPP_LIB_API int fn_ipp_lib_rotateShift_16u(const unsigned short* srcBuffer, int width, int height, int channels, double angle,
										   int shiftX, int shiftY, unsigned short* dstBuffer);