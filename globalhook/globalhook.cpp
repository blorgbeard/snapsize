// globalhook.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <windows.h>
#include <string.h>


//
// Store the application instance of this module to pass to
// hook initialization. This is set in DLLMain().
//
HINSTANCE g_appInstance = NULL;

const char* PROP_HOOK_DESTINATION = "PROP_HOOK_DESTINATION";
const char* MSG_SNAPSIZE_HOOK_REPLACED = "MSG_SNAPSIZE_HOOK_REPLACED";
const char* MSG_SNAPSIZE_CALLWNDPROC = "MSG_SNAPSIZE_CALLWNDPROC";
const char* MSG_SNAPSIZE_CALLWNDPROC_PARAMS = "MSG_SNAPSIZE_CALLWNDPROC_PARAMS";


HHOOK hookCallWndProc = NULL;

int GetNumber(int a, int b) {
	return a + b;
}


static LRESULT CALLBACK CallWndProcHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code >= 0)
	{
		UINT msg1 = 0;
		UINT msg2 = 0;

		msg1 = RegisterWindowMessage(MSG_SNAPSIZE_CALLWNDPROC);
		msg2 = RegisterWindowMessage(MSG_SNAPSIZE_CALLWNDPROC_PARAMS);

		HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), PROP_HOOK_DESTINATION);

		CWPSTRUCT* pCwpStruct = (CWPSTRUCT*)lparam;

		if (msg1 != 0 && pCwpStruct->message != msg1 && pCwpStruct->message != msg2)
		{
			SendNotifyMessage(dstWnd, msg1, (WPARAM)pCwpStruct->hwnd, pCwpStruct->message);
			SendNotifyMessage(dstWnd, msg2, pCwpStruct->wParam, pCwpStruct->lParam);
		}
	}

	return CallNextHookEx(hookCallWndProc, code, wparam, lparam);
}

bool InitializeCallWndProcHook(int threadID, HWND destination)
{
	if (g_appInstance == NULL)
	{
		return false;
	}

	if (GetProp(GetDesktopWindow(), PROP_HOOK_DESTINATION) != NULL)
	{
		SendNotifyMessage(
			(HWND)GetProp(GetDesktopWindow(), PROP_HOOK_DESTINATION), 
			RegisterWindowMessage(MSG_SNAPSIZE_HOOK_REPLACED), 0, 0);
	}

	SetProp(GetDesktopWindow(), PROP_HOOK_DESTINATION, destination);

	hookCallWndProc = SetWindowsHookEx(WH_CALLWNDPROC, (HOOKPROC)CallWndProcHookCallback, g_appInstance, threadID);
	return hookCallWndProc != NULL;
}

void UninitializeCallWndProcHook()
{
	if (hookCallWndProc != NULL)
		UnhookWindowsHookEx(hookCallWndProc);
	hookCallWndProc = NULL;
}
