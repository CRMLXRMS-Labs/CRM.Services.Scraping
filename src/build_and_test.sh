#!/bin/bash

# Define project and coverage directory
SOLUTION_FILE="src.sln"
TEST_PROJECT="CRMScraper.Tests/CRMScraper.Tests.csproj"
COVERAGE_DIRECTORY="coverage"

# Clean up previous coverage reports
rm -rf $COVERAGE_DIRECTORY

# Step 1: Build the solution
echo "Building the solution..."
dotnet build $SOLUTION_FILE --configuration Debug

# Step 2: Run tests and collect coverage
echo "Running tests and collecting coverage..."
dotnet test $TEST_PROJECT --configuration Debug --collect:"XPlat Code Coverage" --results-directory $COVERAGE_DIRECTORY

# Step 3: Check if the coverage file was generated
COVERAGE_FILE=$(find $COVERAGE_DIRECTORY -name "*.xml" | head -n 1)

if [ -f "$COVERAGE_FILE" ]; then
    echo "Generating HTML coverage report..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
    reportgenerator "-reports:$COVERAGE_FILE" "-targetdir:$COVERAGE_DIRECTORY/coverage-html" "-reporttypes:Html"

    # Step 4: Open the coverage report in the default browser
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
