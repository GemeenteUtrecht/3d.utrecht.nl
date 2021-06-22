import requests
import urllib.request, json 
from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry

import uuid
from concurrent.futures import ThreadPoolExecutor, as_completed
import os.path
from os import path

max_jobs = 10

#Amsterdam
#bbox_min_x = 109000
#bbox_min_y = 474000
#bbox_max_x = 141000
#bbox_max_y = 501000

#Utrecht
bbox_min_x = 123000
bbox_min_y = 443000
bbox_max_x = 146000
bbox_max_y = 464000

skip_existing = False

#Example url to inspect in browser: https://data.3dbag.nl/api/BAG3D_v2/wfs?version=1.1.0&request=GetFeature&typename=BAG3d_v2:bag_tiles_3k&outputFormat=application/json&srsname=EPSG:28992&bbox=54938.555154899994,411133.16831151,125417.2751549,482365.16831151,EPSG:28992
wfs_url = f"https://data.3dbag.nl/api/BAG3D_v2/wfs?version=1.1.0&request=GetFeature&typename=BAG3d_v2:bag_tiles_3k&outputFormat=application/json&srsname=EPSG:28992&bbox={bbox_min_x},{bbox_min_y},{bbox_max_x},{bbox_max_y},EPSG:28992"

download_path = "https://data.3dbag.nl/cityjson/v21031_7425c21b/3dbag_v21031_7425c21b_{}.json"
local_filename = "{}_{}_bbox_{}_{}_{}_{}.json"
local_path = os.path.dirname(os.path.realpath(__file__)) + "\\downloaded_tiles\\"

def download_tile(url, file_name):
    try:
        #skip stuff we already downloaded
        if skip_existing and os.path.isfile(file_name):
            return "- " + file_name
            
        #download the download URL, with a delay and retry for when we get blocked
        session = requests.Session()
        retry = Retry(connect=3, backoff_factor=1.0)
        adapter = HTTPAdapter(max_retries=retry)
        session.mount('http://', adapter)
        session.mount('https://', adapter)
        downloaded_obj = session.get(url, stream=True)
        
        #if downloaded data was enough to be of use
        if len(downloaded_obj.content) > 15:
            open(file_name, 'wb').write(downloaded_obj.content)
            return "+ " + file_name
            
        return "x " + file_name
        #return downloaded_obj.status_code
    except requests.exceptions.RequestException as exception:
       return exception

def parse_wfs_json():
    #create the files its containing folder if it doenst exist
    if not os.path.exists(os.path.dirname(local_path)):
        try:
            os.makedirs(os.path.dirname(local_path))
        except OSError as exc: # Guard against race condition
            if exc.errno != errno.EEXIST:
                raise  

    with urllib.request.urlopen(wfs_url) as url:
        print(wfs_url)
        data = json.loads(url.read().decode())
        
        #Now do the downloads in multiple threads for speeeed
        threads = []
        with ThreadPoolExecutor(max_workers=max_jobs) as executor:
            print("Starting threads")
            print("Found features:" + str(data["totalFeatures"])) 
            for feature in data["features"]:   
                tile_id = feature["properties"]["tile_id"]
                cnt = feature["properties"]["cnt"]
                bbox_minx = feature["bbox"][0]
                bbox_miny = feature["bbox"][1]
                bbox_maxx = feature["bbox"][2]
                bbox_maxy = feature["bbox"][3]
                
                download_url = download_path.format(tile_id)
                print(feature["properties"]["tile_id"] + " -> " + download_url)
                threads.append(executor.submit(download_tile, download_path.format(tile_id), local_path + local_filename.format(tile_id,cnt,bbox_minx,bbox_miny,bbox_maxx,bbox_maxy)))
            for task in as_completed(threads):
                print(task.result()) 

parse_wfs_json()