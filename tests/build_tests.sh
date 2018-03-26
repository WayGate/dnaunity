echo "Building tests.."
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CSC="C:/Program Files/Unity/Editor/Data/Mono/bin/gmcs.bat"
mkdir -p "${DIR}/bin"
while IFS= read -r -d $'\0' filepath; do
  filename=$(basename "$filepath")
  ext="${filename##*.}"
  name="${filename%.*}"	
  echo "Building ${filepath}"
  "$CSC" "$filepath" -out:"${DIR}/bin/${name}.exe" -unsafe
done < <(find $DIR -type f -name "*.cs" -print0)
echo "Done building tests"
