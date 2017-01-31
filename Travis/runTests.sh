#! /bin/sh
project="unity-sdk-travis"

echo "Attempting to run Unity integration tests"

rm -f testLog.log

/Applications/Unity/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -nographics \
    -silent-crashes \
    -force-free \
    -projectPath $(pwd)/TravisBuild/Project/SparkUnity \
    -executeMethod UnityTest.Batch.RunIntegrationTests \
    -testscenes=SparkIntegrationTests \
    -targetPlatform=StandaloneOSXIntel \
    -logFile $(pwd)/testLog.log \
    -quit

cat testLog.log | grep "All tests passed"

if [ $? = 0 ] ; then
    exit 0
else
    cat testLog.log
    echo "Tests Failed!"
    exit 1
fi
