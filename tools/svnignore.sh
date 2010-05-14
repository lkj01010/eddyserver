#/!bin/bash
#set svnignored list

set -x
for ignorefile in `find -iname .svnignore -exec echo \{\} \;`
do svn ps svn:ignore -F $ignorefile `dirname $ignorefile`;
done
