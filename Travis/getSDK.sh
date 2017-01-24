#! /bin/sh
project="unity-sdk-travis"

echo "Attempting to fetch Cisco Spark SDK"
mkdir -p Project/
git clone https://github.com/RichLogan/CiscoSpark-UnitySDK.git Project
echo "SDK Ready"

echo "Attempting to fetch Unity Test Tools"
curl -o unityTestTools.zip https://bitbucket.org/Unity-Technologies/unitytesttools/get/295e5eea2eee.zip
if [ $? = 0] ; then
    echo "Preparing Test Tools..."
    unzip unityTestTools.zip
    mv Assts/UnityTestTools Project/SparkUnity/Assets/
    if [$? = 0]; then
        echo "Test Tools Ready"
        exit 0
    fi
fi

