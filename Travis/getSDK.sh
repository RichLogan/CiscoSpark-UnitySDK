#! /bin/sh
project="unity-sdk-travis"

mkdir TravisBuild
cd TravisBuild
echo "Attempting to fetch Cisco Spark SDK"
mkdir -p Project/
git clone --recursive https://github.com/RichLogan/CiscoSpark-UnitySDK.git Project
cd Project
git checkout feature/integration-tests
echo "SDK Ready"
