from dirsync import sync
source_path = '/3DAmsterdam.fork/3DAmsterdam'
target_path = '3DNetherlands'
pattern = ('.*3DUtrecht.*$','.*Fullscreen3DUtrecht.*$','^Library.*$','^Temp.*$', '.*Plugins.*.meta$')
sync(source_path, target_path, 'sync', purge = True, create = True, verbose = True, ignore= pattern) 
