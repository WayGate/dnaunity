echo "Running tests.."
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
TEST="./bin/Debug/test.exe"
PASSED=0
FAILED=0
NORMAL=$(tput sgr0)
RED=$(tput setaf 1)
GREEN=$(tput setaf 2)
while IFS= read -r -d $'\0' filepath; do
  filename=$(basename "$filepath")
  ext="${filename##*.}"
  name="${filename%.*}"	
  printf "${NORMAL}Running test ${name} - "
  "$TEST" "${name}.exe"
  if [ $? -eq 100 ]; then
    printf "${GREEN}PASSED${NORMAL}\n"
    let "PASSED++"
  else
	printf "${RED}FAILED${NORMAL}\n"
    let "FAILED++"
  fi
done < <(find $DIR -type f -name "*.cs" -print0)
echo "All tests completed - ${GREEN}${PASSED} passed${NORMAL} ${RED}${FAILED} failed${NORMAL}"
