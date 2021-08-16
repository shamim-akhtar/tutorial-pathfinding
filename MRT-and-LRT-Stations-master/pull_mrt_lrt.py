#!/usr/bin/env python

import onemap
import urllib
import httplib2
import json
import gzip
import cPickle
import pyproj
import pandas as pd

url = onemap.GetURL('RestServiceUrl')
svy21 = pyproj.Proj(init='epsg:3414')

def download_stuff(what):
    data = []
    rset = 1

    while True:
        rurl = url + \
            urllib.quote_plus('SEARCHVAL LIKE \'${}$\''.format(what)) + \
        '&' + urllib.urlencode({
            'rset': str(rset),
            'otptFlds': 'POSTALCODE,CATEGORY',
        })
        print rurl

        h = httplib2.Http('.cache')
        _,content = h.request(rurl)

        print content

        obj = json.loads(content)
        data.append(obj)

        if len(obj['SearchResults']) == 1:
            break

        rset += 1

    return data

data = download_stuff('MRT STATION') + download_stuff('LRT STATION')
data = [d for e in data for d in e['SearchResults'][1:]]
data = [d for d in data if d['SEARCHVAL'].endswith('STATION')]

# exclude the ones starting with the name of a bank
# (these are ATM locations)
bank_names = list(open('./bank_names.txt'))
bank_names = [b.strip() + ' ' for b in bank_names]

data = [d for d in data if not any([d['SEARCHVAL'].upper().startswith(bank_name.upper()) for bank_name in bank_names])]

# output the pickle file
cPickle.dump(data, gzip.open('mrt_lrt.pickle.gz', 'w'))

# output the CSV
def make_csv_row(r):
    lnglat = svy21(float(r['X']), float(r['Y']), inverse=True)

    return {
        'Latitude': lnglat[1],
        'Longitude': lnglat[0],
        'X': float(r['X']),
        'Y': float(r['Y']),
        'Name': r['SEARCHVAL']
    }
    
    
df_data = [make_csv_row(r) for r in data]
df = pd.DataFrame(df_data)
df.to_csv('./mrt_lrt.csv')

