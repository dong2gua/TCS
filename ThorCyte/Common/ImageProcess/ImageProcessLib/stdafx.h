// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

// TODO: reference additional headers your program requires here
// C/C++ standard lib header files 
#include <stdio.h>
#include <stdlib.h>
#include <list>
#include <vector>
#include <algorithm>
using namespace std;

//Intel IPP header files
#define _IPP_SEQUENTIAL_STATIC
#include "ipp.h"
