#this sets variables (named after the directories) where a dockerfile sits inside the directory and a git change has been made to the subdirectory

directories=$(ls -d */)
declare -a dockerContainerToCheck=()

for i in ${directories[@]}
do
   dockerfile=$(ls "${i}Dockerfile")
   [[ ${dockerfile} =~ "Dockerfile" ]] && dockerContainerToCheck+=(${i}) && echo "Found dockerfile in ${i}"
done

DIFFS="$(git diff HEAD HEAD~ --name-only)"
echo "found diffs.."
echo ${DIFFS}

echo "checking directories.."

for i in ${dockerContainerToCheck[@]}
do
   echo ${i}
   devopsVariableName=${i%/}
   [[ "${DIFFS[@]}" =~ "${i}" ]] && echo "##vso[task.setvariable variable=${devopsVariableName};isOutput=true;]True"
done

exit 0