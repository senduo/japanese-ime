#!/bin/bash

mkdir tmp dest

for path in `ls -1 src/*.txt`
do
  dest="tmp/${path#src/}"
  echo $dest
  grep -e '.*。$' $path | grep -e '^[^*:]' > $dest
done

for path in `ls -1 tmp/*.txt`
do
  dest="dest/${path#tmp/}"
  echo $dest
  kytea $path > $dest
done


