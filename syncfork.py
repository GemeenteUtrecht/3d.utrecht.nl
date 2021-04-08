from dirsync import sync
source_path = '3DNetherlands/Assets/Netherlands3D'
target_path = '/3DAmsterdam.fork/3DAmsterdam/Assets/Netherlands3D'
sync(source_path, target_path, 'sync', purge = True, create = True, verbose = True, ctime=True) 
