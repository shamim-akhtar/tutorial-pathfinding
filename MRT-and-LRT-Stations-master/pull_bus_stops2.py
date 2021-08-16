#!/usr/bin/env python

import onemap
import urllib
import httplib2
import json
import gzip
import cPickle
import re
import pandas as pd
import pyproj
import data_mall
import numpy as np

url = onemap.GetURL('RestServiceUrl')
svy21 = pyproj.Proj(init='epsg:3414')

def download_stuff():
    """
    Bus stop codes are 5-digit.
    This searches all bus stop codes from 00000
    to 99999
    """
    data = []

    for start in range(0,100):
        rset = 1

        while True:

            rurl = url + \
                urllib.quote_plus('SEARCHVAL LIKE \'' + '{0:02d}'.format(start) + '$BUS STOP$\'') + \
            '&' + urllib.urlencode({
                'rset': str(rset),
                'otptFlds': 'POSTALCODE,CATEGORY',
            })
            print rurl

            h = httplib2.Http('.cache')
            _,content = h.request(rurl)

            obj = json.loads(content)

            if len(obj['SearchResults']) == 1:
                break

            data = data + obj['SearchResults'][1:]
            print len(obj['SearchResults'])
            rset += 1

    return data

# This is the one map data, but it is missing the Malaysian bus stops
data = download_stuff()

# Download stops from data mall
data_mall.authenticate('1hMEqSwhQWWRo88SupMVzQ==', '6b2ffab0-5916-4a20-a0d7-8f9824627d7b')
dm_data = data_mall.get('BusStops', 0)
dm_data = [d for d in dm_data if d['Longitude'] != 0]
dm_data = [d for d in dm_data if re.match('[0-9]{5}', d['BusStopCode']) != None]

for d in dm_data:
    X,Y = svy21(d['Longitude'], d['Latitude'])
    data.append({
        'SEARCHVAL': '%s (BUS STOP)' % (d['BusStopCode']),
        'X': X,
        'Y': Y
    })

# Save the pickle file
cPickle.dump(data, gzip.open('busstops2.pickle.gz', 'w'))

# Output the CSV
def bus_stop_code(d):
    matches = re.match('([0-9]{5}) \(BUS STOP\)', d['SEARCHVAL'])

    if matches != None:
        return matches.group(1)
    else:
        raise ValueError

def make_row(d):
    lnglat = svy21(float(d['X']), float(d['Y']), inverse=True)

    return {
        'Name': bus_stop_code(d),
        'X': d['X'],
        'Y': d['Y'],
        'Latitude': lnglat[1],
        'Longitude': lnglat[0],
    }

df_data = [make_row(r) for r in data]
df = pd.DataFrame(df_data)
df.to_csv('./bus_stops.csv')
