rmdir /s /q .\LPRx86
rmdir /s /q .\LPRx64

delete .\LPRx86.zip
delete .\LPRx64.zip

mkdir .\LPRx86
mkdir .\LPRx64

xcopy .\Release .\LPRx86 /E/H/C/I
xcopy .\x64\Release .\LPRx64 /E/H/C/I

path C:\Program Files\7-Zip\

7z a -tzip .\LPRx86.zip .\LPRx86
7z a -tzip .\LPRx64.zip .\LPRx64

rmdir /s /q .\LPRx86
rmdir /s /q .\LPRx64