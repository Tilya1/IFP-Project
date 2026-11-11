#!/bin/sh
cd "$(dirname "$0")"
mkdir -p build

if [ "$(uname)" = "Darwin" ]; then
    clang -shared -fPIC -O2 -o build/libquotenative.dylib quote_native.c
    echo "Built native/build/libquotenative.dylib"
else
    gcc -shared -fPIC -O2 -o build/libquotenative.so quote_native.c
    echo "Built native/build/libquotenative.so"
fi
