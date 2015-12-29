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
IPP_LIB_API int fnipp_lib_resize_16uC1(unsigned short* srcBuffer, int srcWidth, int srcHeight,
								 unsigned short* dstBuffer, int dstWidth, int dstHeight);
IPP_LIB_API int fnipp_lib_AddConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer);
IPP_LIB_API int fnipp_lib_SubConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer);

IPP_LIB_API int fnipp_lib_MulConstant_16uC1(unsigned short value, unsigned short* srcBuffer, int width, int height, 
											unsigned short* dstBuffer);


//not export function
void MoveBuffer_16u(unsigned short* srcBuffer, unsigned short* dstBuffer, int dstWidth, int dstHeight);