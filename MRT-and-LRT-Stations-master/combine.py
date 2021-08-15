import pandas as pd

bus_stops = pd.read_csv('./bus_stops.csv')
stations = pd.read_csv('./mrt_lrt.csv')

def strip_mrtlrt(row):
    assert(
        row['Name'].endswith(' MRT STATION') or\
        row['Name'].endswith(' LRT STATION')
    )

    l = len(' MRT STATION')
    return row['Name'][:-l]

stations['Name'] = stations.apply(strip_mrtlrt, axis=1)

combined = pd.concat((bus_stops, stations))
combined = combined.drop(['Unnamed: 0'], axis=1)
print combined
combined.reset_index(drop=True, inplace=True)

combined.to_csv('./combined.csv')

