#!/usr/bin/env bash

set -eu
set -o pipefail

dotnet fsi ./_build/build.fsx  -t "$@"
