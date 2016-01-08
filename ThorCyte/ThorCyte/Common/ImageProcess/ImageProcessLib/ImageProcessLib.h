#pragma once
#include "stdafx.h"

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
IPP_LIB_API unsigned __int64 fnipp_lib_getCpuClocks();
IPP_LIB_API int fnipp_lib_resize_16u(unsigned short* srcBuffer, int srcWidth, int srcHeight,
									 unsigned short* dstBuffer, int dstWidth, int dstHeight, int channels);
IPP_LIB_API int fnipp_lib_addConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels, unsigned short maxValue);

IPP_LIB_API int fnipp_lib_add_16u(unsigned short* srcBuffer1, unsigned short* srcBuffer2, 
								  int width, int height, unsigned short* dstBuffer, int channels, unsigned short maxValue);
IPP_LIB_API int fnipp_lib_subConstant_16u(unsigned short value, unsigned short* srcBuffer, int width, 
										  int height, unsigned short* dstBuffer, int channels);

IPP_LIB_API int fnipp_lib_sub_16u(unsigned short* minuendBuffer, unsigned short* subtracterBuffer, 
									int width, int height, unsigned short* dstBuffer, int channels);

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

IppStatus saturate(unsigned short* buffer, int width, int height, int channels, unsigned short maxValue);