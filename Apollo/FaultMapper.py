#!python
# Plot probababilit field, extracted fault traces, fault bisectors and throw sample
# points on a map with geographical co-ordinates and a pixel grid.
# 
# Usage:
# 
# python map.py horizon

import re
import sys
import subprocess

cfloat = r'([+-]?)(?=\d|\.\d)\d*(\.\d*)?([Ee]([+-]?\d+))?'
minmax = '<(?P<minx>' + cfloat + ')/(?P<maxx>' + cfloat + r')>\s+<(?P<miny>'+ cfloat + ')/(?P<maxy>' + cfloat + ')>'
grdinfo = '-R(?P<minx>' + cfloat + ')/(?P<maxx>' + cfloat + ')/(?P<miny>' + cfloat + ')/(?P<maxy>' + cfloat + ')'
grdz = '-T(?P<minz>' + cfloat + ')/(?P<maxz>' + cfloat + ')/(?P<incz>' + cfloat + ')'

def plotMap(base_file):
    grid_file = base_file + '.grd'
    centerlines_file = base_file + '_faults.poly'
    bisectors_file = base_file + '_bisectors.poly'
    labels_file = base_file + '_labels.xy'
    strands_file = base_file + '_strands.poly'
    mono_file = base_file + '_mono.poly'
    faces_file = base_file + '_faces.poly'
    
    # Determine union bounding box of the polygon files and the grids
    
    # Determine centerline file bounding box
    output = subprocess.Popen(["minmax", centerlines_file, "-M%"], stdout=subprocess.PIPE).communicate()[0]
    match = re.compile(minmax).search(output)
    if not match:
        sys.stderr.write("Error computing minmax for %s" % poly_file)
        sys.stderr.write(output)
        sys.exit(1)
    fields = match.groupdict()
    center_poly_minx = float(fields['minx'])
    center_poly_maxx = float(fields['maxx'])
    center_poly_miny = float(fields['miny'])
    center_poly_maxy = float(fields['maxy'])
    print center_poly_minx, center_poly_maxx, center_poly_miny, center_poly_maxy
    
    # Determine centerline file bounding box
    output = subprocess.Popen(["minmax", bisectors_file, "-M%"], stdout=subprocess.PIPE).communicate()[0]
    match = re.compile(minmax).search(output)
    if not match:
        sys.stderr.write("Error computing minmax for %s" % poly_file)
        sys.stderr.write(output)
        sys.exit(1)
    fields = match.groupdict()
    bisector_poly_minx = float(fields['minx'])
    bisector_poly_maxx = float(fields['maxx'])
    bisector_poly_miny = float(fields['miny'])
    bisector_poly_maxy = float(fields['maxy'])
    print bisector_poly_minx, bisector_poly_maxx, bisector_poly_miny, bisector_poly_maxy
    
    # Determine grid file bounding box
    output = subprocess.Popen(["grdinfo", grid_file, "-I1"], stdout=subprocess.PIPE).communicate()[0]
    match = re.compile(grdinfo).search(output)
    if not match:
        sys.stderr.write("Error computing minmax for %s" % grid_file)
        sys.stderr.write(output)
        sys.exit(1)
    fields = match.groupdict()
    grid_minx = float(fields['minx'])
    grid_maxx = float(fields['maxx'])
    grid_miny = float(fields['miny'])
    grid_maxy = float(fields['maxy'])
    print grid_minx, grid_maxx, grid_miny, grid_maxy
    
    minx = min(center_poly_minx, bisector_poly_minx, grid_minx)
    miny = min(center_poly_miny, bisector_poly_miny, grid_miny)
    maxx = max(center_poly_maxx, bisector_poly_maxx, grid_maxx)
    maxy = max(center_poly_maxy, bisector_poly_maxy, grid_maxy)
    print minx, maxx, miny, maxy
    
    # Determine grid z range
    output = subprocess.Popen(["grdinfo", grid_file, "-T1"], stdout=subprocess.PIPE).communicate()[0]
    match = re.compile(grdz).search(output)
    if not match:
        sys.stderr.write("Error computing minmax for %s" % grid_file)
        sys.stderr.write(output)
        sys.exit(1)
    fields = match.groupdict()
    grid_minz = float(fields['minz'])
    grid_maxz = float(fields['maxz'])
    
    # Determine grid size in nodes
    output = subprocess.Popen(["grdinfo", grid_file, "-C"], stdout=subprocess.PIPE).communicate()[0]
    fields = output.split()
    nx = int(fields[-2])
    ny = int(fields[-1])
    print nx, ny
    
    aspect_ratio = (maxy - miny) / (maxx - minx)
    
    plot_width_cm = 15
    plot_height_cm = plot_width_cm * aspect_ratio
    
    print plot_height_cm
    
    postscript_file = base_file + '.ps'
    cpt_file = base_file + '.cpt'
    
    # Make the histogram equalized colour table
    returncode = subprocess.call("grd2cpt %s -Z -Chaxby > %s" % (grid_file, cpt_file), shell=True)
    
    # Plot the image map
    returncode = subprocess.call("grdimage %s -C%s -JX%f -R%f/%f/%f/%f -E100 -Q -P -K > %s" % (grid_file, cpt_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)

    # Overlay the grid cells
    #returncode = subprocess.call('psbasemap -JX%f -R0/%d/0/%d -Bg10/g10 --GRID_PEN_PRIMARY=gray -P -O -K >> %s' % (plot_width_cm, nx, ny, postscript_file), shell=True)
    
    # Plot the faces of the mesh
    returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -W0/red -L -P -O -K >> %s" % (faces_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
    
    # Plot the center line polyline map
    returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -W1/255/255/255 -P -O -K >> %s" % (centerlines_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
         
    # Plot the monotonization edges
    #returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -W2/255/255/255 -W1,yellow,- -P -O -K >> %s" % (mono_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
   
    # Plot the bisector line polygon map
    #returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -P -O -K >> %s" % ("bisector_edges_10", plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)

    # Plot the strands line polygon map
    #returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -W1/0/120/60 -P -O -K >> %s" % (strands_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
    
    # Plot the bisector labels
    ##returncode = subprocess.call("pstext %s -JX%f -R%f/%f/%f/%f -Dj0.02/0.02v0.25,yellow -Gyellow -P -O -K >> %s" % (labels_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)

    # Plot the bisector points map
    ##returncode = subprocess.call("psxy %s -JX%f -R%f/%f/%f/%f -M%% -Sc0.01 -Wred -P -O -K >> %s" % (bisectors_file, plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
    
    # Plot the base map
    returncode = subprocess.call('psbasemap -JX%f -R%f/%f/%f/%f -B10000/10000::WS --D_FORMAT=%%7.0f -P -O -K >> %s' % (plot_width_cm, minx, maxx, miny, maxy, postscript_file), shell=True)
    
    # Overlay the grid cells
    returncode = subprocess.call('psbasemap -JX%f -R0/%d/0/%d -Bf10a50/f10a50:."%s":EN -P -O >> %s' % (plot_width_cm, nx, ny, base_file, postscript_file), shell=True)
        
if __name__ == '__main__':
    base_file = sys.argv[1]
    plotMap(base_file)
    
