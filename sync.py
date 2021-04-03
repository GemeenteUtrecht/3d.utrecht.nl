from dirsync import sync
source_path = '/3DAmsterdam.fork/3DAmsterdam'
target_path = '3DNetherlands'

sync(source_path, target_path, 'sync', purge = True, create = True, verbose = True) #for syncing one way
#sync(target_path, source_path, 'sync') #for syncing the opposite way