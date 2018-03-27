echo "Running tests.."

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
TEST="./bin/Debug/test.exe"
PASSED=0
FAILED=0
SKIPPED=0
NORMAL=$(tput sgr0)
RED=$(tput setaf 1)
GREEN=$(tput setaf 2)
YELLOW=$(tput setaf 3)

IFS=$'\r\n' GLOBIGNORE='*' command eval  'TESTS_TO_SKIP=($(cat $DIR/tests_to_skip))'
echo "Skip: ${TESTS_TO_SKIP[@]}"

contains () {
  local e match="$1"
  shift
  for e; do [[ "$e" == "$match" ]] && return 0; done
  return 1
}

while IFS= read -r -d $'\0' filepath; do
  filename=$(basename "$filepath")
  ext="${filename##*.}"
  name="${filename%.*}"	
  printf "${NORMAL}Running test ${name} - "
  if $(contains "${name}" "${TESTS_TO_SKIP[@]}"); then
    printf "${YELLOW}SKIPPED${NORMAL}\n"
    let "SKIPPED++"
  else 
    "${TEST}" "${name}.exe"
    if [ $? -eq 100 ]; then
      printf "${GREEN}PASSED${NORMAL}\n"
      let "PASSED++"
    else
  	printf "${RED}FAILED${NORMAL}\n"
      let "FAILED++"
    fi
  fi
done < <(find $DIR -type f -name "*.cs" -print0)

echo "All tests completed - ${GREEN}${PASSED} passed${NORMAL} ${RED}${FAILED} failed${NORMAL} ${YELLOW}${SKIPPED} skipped${NORMAL}"
