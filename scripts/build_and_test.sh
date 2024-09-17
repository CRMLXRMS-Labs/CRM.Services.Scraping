#!/bin/bash

SOLUTION_FILE="src.sln"
TEST_PROJECT="CRMScraper.Tests/CRMScraper.Tests.csproj"
COVERAGE_DIRECTORY="coverage"

rm -rf $COVERAGE_DIRECTORY

echo "Building the solution..."
dotnet build $SOLUTION_FILE --configuration Debug

echo "Running tests and collecting coverage..."
dotnet test $TEST_PROJECT --configuration Debug --collect:"XPlat Code Coverage" --results-directory $COVERAGE_DIRECTORY

COVERAGE_FILE=$(find $COVERAGE_DIRECTORY -name "*.xml" | head -n 1)

if [ -f "$COVERAGE_FILE" ]; then
    echo "Generating HTML coverage report..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
    reportgenerator "-reports:$COVERAGE_FILE" "-targetdir:$COVERAGE_DIRECTORY/coverage-html" "-reporttypes:Html"

    COVERAGE_REPORT="$COVERAGE_DIRECTORY/coverage-html/index.html"
    echo "Opening coverage report in browser..."
    if [ -f "$COVERAGE_REPORT" ]; then
        xdg-open "$COVERAGE_REPORT"
    else
        echo "Error: Coverage report HTML not found!"
    fi
else
    echo "Error: Coverage file not found!"
fi
