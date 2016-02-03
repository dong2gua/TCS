// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <list>
using namespace std;
// TODO: reference additional headers your program requires here
#define _IPP_SEQUENTIAL_STATIC
#include "ipp.h"
#include <opencv2/opencv.hpp>
using namespace cv;