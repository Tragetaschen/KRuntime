@Echo OFF
SETLOCAL ENABLEDELAYEDEXPANSION
SET ERRORLEVEL=
:: K [command] [args]
::   command :  Required - Name of the command to execute
::   args : Optional - Any further args will be passed directly to the command.

:: e.g. To compile the app in the current folder:
::      C:\src\MyApp\>K build

REM <dev>
@Echo ON
REM </dev>

SET KRE_BIN=%~dp0

SET I=1
:LOOP
IF (%1)==() (
  GOTO END
) ELSE (
  SET ARG=%1
  SET ARGS[!I!]=!ARG:/?="/?"!
  SET /A I+=1
  SHIFT
  GOTO LOOP
)
:END

IF EXIST "%KRE_BIN%k-!ARGS[1]!.cmd" (
  "%KRE_BIN%k-!ARGS[1]!.cmd" !ARGS[2]! !ARGS[3]! !ARGS[4]! !ARGS[5]! !ARGS[6]! !ARGS[7]! !ARGS[8]! !ARGS[9]! 
) ELSE (
  CALL "%KRE_BIN%KLR.cmd" "Microsoft.Framework.ApplicationHost" !ARGS[1]! !ARGS[2]! !ARGS[3]! !ARGS[4]! !ARGS[5]! !ARGS[6]! !ARGS[7]! !ARGS[8]! !ARGS[9]!
)

exit /b %ERRORLEVEL%
ENDLOCAL