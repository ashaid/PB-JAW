import json
from pixels import *
from scipy.sparse.csgraph import shortest_path
from scipy.sparse import csr_matrix
from PIL import ImageDraw, Image, ImageFont
import numpy as np
from sknetwork.utils import edgelist2adjacency
import os
import pathlib
import sys


def edge_list_getter(building):
    edge_list = np.array([])
    if building == "bec":
        edge_list = np.array([
            (1, 1620, 1), (1, 1615, 1), (3, 1, 2), (3, 2, 2), (2, 1720, 1), (4, 3, 2), (4, 5, 2),
            (5, 6, 3), (6, 7, 2), (7, 1800, 1), (7, 8, 2), (8, 1745, 1), (6, 9, 3), (9, 10, 2),
            (10, 1835, 1), (10, 1900, 1), (10, 11, 2), (11, 1845, 1), (11, 1920, 1), (5, 12, 4),
            (12, 13, 2), (13, 1321, 1), (13, 14, 2), (14, 1325, 1), (14, 15, 2), (15, 1409, 1),
            (15, 16, 1), (16, 1420, 1), (16, 1425, 1), (12, 17, 1), (17, 18, 2), (18, 1305, 1),
            (18, 19, 2), (19, 1220, 1), (19, 1225, 1), (17, 21, 3), (21, 22, 2), (22, 1120, 1),
            (22, 20, 2), (20, 1125, 1), (9999, 5, 3)
        ])
    elif building == "locb":
        edge_list = np.array([
            (15, 700, 1), (700, 600, 2), (600, 500, 2), (500, 1100, 2), (1100, 16, 1), (1100, 400, 2),
            (400, 300, 2), (300, 10, 1), (300, 200, 2), (200, 6, 1), (200, 100, 2), (100, 2, 1),
            (100, 9999, 2), (100, 1000, 2), (1000, 5, 1), (1000, 900, 2), (900, 9, 1), (900, 800, 2),
            (700, 800, 2)
        ])
    # elif building == "pft": (first floor)
    #    edge_list = np.array([
    #        (14, 1100, 1), (14, 1200, 1), (14, 1202, 1), (6, 1206, 1), (4, 1256, 1), (6, 1258, 1),
    #        (6, 1262, 1), (13, 1263, 1), (2, 1212, 1), (1, 1216, 1), (1, 1218, 1), (10, 1221, 1),
    #        (9, 1225, 1), (8, 1232, 1), (8, 1236, 1), (7, 1240, 1), (7, 1244, 1), (4, 1246, 1),
    #        (7, 1245, 1), (4, 1253, 1), (16, 1360, 1), (12, 9997, 2), (12, 9998, 2), (10, 9999, 2),
    #        (9999, 1, 2), (1, 2, 2), (3, 2, 2), (13, 3, 2), (14, 13, 2), (4, 3, 2), (5, 4, 2),
    #        (6, 5, 2), (12, 5, 2), (11, 9997, 2), (11, 9, 2), (9, 8, 2), (8, 7, 2), (7, 5, 2),
    #        (10, 9, 2), (16, 15, 2), (17, 16, 2), (9998, 7, 2), (11, 15, 2)
    #    ])

    #2ND FLOOR NOT STARTED
    # elif building == "pft-2": or whatever second floor of pft is called
    #    edge_list = np.array([
    #        (15, 2331, 1), (15, 2341, 1), (14, 2303, 1), (16, 2329, 1), (16, 2326, 1), (17, 2327, 1),
    #        (17, 2324, 1), (17, 2325, 1), (18, 2323, 1), (18, 2321, 1), (18, 2317, 1), (19, 2319, 1),
    #        (19, 2318, 1), (20, 2316, 1), (13, 2345, 1), (13, 2347, 1), (13, 2348, 1), (12, 2350, 1),
    #        (12, 2352, 1), (11, 2354, 1), (11, 2355, 1), (11, 2356, 1), (10, 2367, 1), (9, 2261, 1), (9, 2262, 1), (14, 2304, 1),
    #        (37, 2308, 1), (36, 2150, 1), (36, 2148, 1), (35, 2146, 1), (35, 2144, 1), (34, 2142, 1),
    #        (33, 2140, 1), (36, 2152, 1), (9999, 2154, 1), (35, 2147, 1), (34, 2145, 1), (33, 2141, 1),
    #        (33, 2136, 1), (31, 2132, 1), (31, 2130, 1), (31, 2129, 1), (30, 2127, 1), (30, 2125, 1),
    #        (29, 2123, 1), (30, 2126, 1), (29, 2122, 1), (28, 2120, 1), (28, 2114, 1), (29, 2119, 1),
    #        (28, 2115, 1), (28, 2113, 1), (9, 2260, 1), (8, 2254, 1), (8, 2249, 1), (6, 2243, 1),
    #        (7, 2245, 1), (6, 2242, 1), (4, 2240, 1), (4, 2400, 1), (3, 2213, 1), (5, 2215, 1),
    #        (15, 9998, 2), (16, 15, 2), (14, 15, 2), (17, 16, 2), (18, 17, 2), (19, 18, 2), (20, 19, 2),
    #        (21, 20, 2), (22, 21, 2), (18, 22, 2), (37, 22, 2), (14, 37, 2), (14, 13, 2), (12, 13, 2),
    #        (11, 12, 2), (11, 10, 2), (10, 9, 2), (9, 8, 2), (8, 7, 2), (7, 6, 2), (6, 5, 2), (5, 24, 2), 
    #        (5, 26, 2), (26, 25, 2), (25, 23, 2), (27, 25, 2), (27, 3, 2), (3, 4, 2), (4, 5, 2), (2, 3, 2),
    #        (2, 1, 2), (1, 8, 2), (1, 9999, 2), (24, 9997, 2), (9999, 36, 2), (36, 35, 2), (35, 34, 2), 
    #        (34, 33, 2), (33, 32, 2), (32, 31, 2), (31, 30, 2), (30, 29, 2), (29, 28, 2), (29, 27, 2)
    #       
    #    ])


    elif building == "loc":
        edge_list = np.array([
            (2, 101, 1), (5, 102, 1), (3, 103, 1), (6, 104, 1), (4, 105, 1), (7, 106, 1),
            (4, 107, 1), (7, 108, 1), (4, 109, 1), (7, 110, 1), (4, 111, 1), (7, 112, 1),
            (3, 113, 1), (7, 114, 1), (6, 116, 1), (8, 119, 1), (12, 130, 1), (14, 132, 1),
            (12, 134, 1), (13, 136, 1), (13, 138, 1), (10, 135, 1), (11, 137, 1), (11, 139, 1),
            (11, 141, 1), (11, 143, 1), (10, 145, 1), (10, 147, 1), (15, 148, 1), (16, 146, 1),
            (16, 144, 1), (17, 142, 1), (17, 140, 1), (7, 9997, 2), (4, 9998, 2), (1, 9999, 2),
            (2, 1, 2), (5, 1, 2), (6, 5, 2), (7, 6, 2), (12, 5, 2), (13, 12, 2), (14, 12, 2),
            (15, 14, 2), (16, 15, 2), (17, 16, 2), (3, 2, 2), (8, 2, 2), (4, 3, 2), (9, 8, 2),
            (10, 9, 2), (11, 10, 2)
        ])
    elif building == "loc2":
        edge_list = np.array([
            (2, 284, 1), (1, 285, 1), (2, 282, 1), (4, 280, 1), (5, 276, 1), (5, 277, 1),
            (6, 237, 1), (6, 239, 1), (11, 235, 1), (11, 241, 1), (10, 243, 1),
            (10, 244, 1), (9, 240, 1), (7, 232, 1), (7, 9998, 2), (1, 9999, 2), (8, 7, 2),
            (9, 8, 2), (10, 9, 2), (11, 8, 2), (2, 1, 2), (3, 2, 2), (4, 3, 2), (5, 4, 2),
            (6, 3, 2), (6, 11, 2), (1, 7, 3)
        ])

    return edge_list


# calculate shortest path nodes
# draw nodes
# draw a line connecting nodes
# watermark code provided by https://www.tutorialspoint.com/python_pillow/python_pillow_creating_a_watermark.htm
def path_finder(path, new_name, building, start, dest):
    edge_list = edge_list_getter(building)
    adjacency = edgelist2adjacency(edge_list, undirected=True)
    adjacency = csr_matrix(adjacency)
    dist_matrix, predecessors = shortest_path(adjacency, directed=False, method='auto', return_predecessors=True)

    # grab json data
    with open(str(pathlib.Path().absolute()) + f"\\python\\json_buildings\\" + building + ".json") as f:
        data = json.load(f)
    # fill out desired nodes

    print(get_path(predecessors, start, dest))

    nodes = []
    for node in get_path(predecessors, start, dest):
        node = Node(find(data, str(node), 'x1'), find(data, str(node), 'y1'), find(data, str(node), 'x2'),
                    find(data, str(node), 'y2'))
        nodes.append(node)

    # actual drawing
    font_path = str(pathlib.Path().absolute().parent) + f"\\pb-jaw\\wwwroot\\css\\Font\\TIMES.TTF"
    # grab correct dictionary and room number
    building_dict = find_dict(building)
    # variables for watermark
    water_mark = "PB-JAW"
    font_size = 100
    margin = 5

    # open image file
    with Image.open(open(path, 'rb')) as im:
        font = ImageFont.truetype(font_path, font_size)
        # draw rectangle
        draw = ImageDraw.Draw(im)
        draw.rectangle([(building_dict[str(start)][0], building_dict[str(start)][1]),
                        (building_dict[str(start)][2], building_dict[str(start)][3])], (0, 0, 255, 15))
        draw.rectangle([(building_dict[str(dest)][0], building_dict[str(dest)][1]),
                        (building_dict[str(dest)][2], building_dict[str(dest)][3])], (255, 0, 0, 15))

        # draw text, xy pixels, text, fill color, font (drawing room number on image)
        # draw.text((25, 74), "Room:" + room_number, fill='black', font=font)

        # calculate x,y coordinates of text
        width, height = im.size
        text_width, text_height = draw.textsize(water_mark, font)
        x = width - text_width - margin
        y = height - text_height - margin
        # draw watermark
        draw.text((x, y), water_mark, font=font, fill='black')

        # x center = (x1 + x2) / 2
        # y center = (y1 + y2) / 2
        for i in range(len(nodes)):
            if i == len(nodes) - 1:
                break
            else:
                draw.line([(nodes[i].x1 + nodes[i].x2) / 2, (nodes[i].y1 + nodes[i].y2) / 2,
                           (nodes[i + 1].x1 + nodes[i + 1].x2) / 2, (nodes[i + 1].y1 + nodes[i + 1].y2) / 2],
                          fill="black", width=5)

        del draw

    # grabs path to created image
    cd = str(pathlib.Path().absolute().parent) + f"\\pb-jaw\\wwwroot\\created\\{new_name}"

    try:
        save_image(im, cd)
    except Exception as err:
        print(format(err))

    print(f"\t* Saved new file at {cd} *")


def find(json_object, name, element):
    return [obj for obj in json_object if obj['name'] == name][0][element]


def get_path(predecessors, i, j):
    path = [j]
    k = j
    while predecessors[i, k] != -9999:
        path.append(predecessors[i, k])
        k = predecessors[i, k]
    return path[::-1]


# method used for converting pixel dictionary to a correctly formatted json file
def convert():
    json_begin = '['
    json_end = ']'
    with open("pixels.json", "w+") as outfile:
        files = os.listdir(os.curdir)
        outfile.write(json_begin)

        for name, pixels in (bec_dict.items()):
            data = {
                "name": name,
                "x1": pixels[0],
                "y1": pixels[1],
                "x2": pixels[2],
                "y2": pixels[3]
            }
            json.dump(data, outfile, indent=4)
            if name != files[-1]:
                outfile.write(',')
        outfile.write(json_end)


# This method opens an image
#
# return: new_image
#
# parameters:
# path              image location
# 
# @author Anthony Shaidaee
def open_image(path):
    new_image = Image.open(path)
    return new_image


# This method saves the image
#
# return type: void
#
# parameters:
# image             image
# path              path to save
# 
# @author Anthony Shaidaee
def save_image(image, path):
    image.save(path, "JPEG")


# This method grabs the correct building dictionary
#
# return: building_dict
#
# parameters:
# building_dict     corresponding dictionary in pixels.py
#
# @author Anthony Shaidaee
def find_dict(building_dict):
    if building_dict == "bec":
        building_dict = bec_dict
    elif building_dict == "pft":
        building_dict = pft_dict
    elif building_dict == "loc":
        building_dict = loc_dict
    # elif building_dict == "pft2":
    # building_dict = pft2_dict
    elif building_dict == "loc2":
        building_dict = loc2_dict
    elif building_dict == "locb":
        building_dict = locb_dict
    return building_dict


# node class used to store node values
class Node:
    def __init__(self, x1, y1, x2, y2, *argv, **kwargs):
        self.x1 = x1
        self.y1 = y1
        self.x2 = x2
        self.y2 = y2


def main(path, new_path, dictionary, start, dest):
    print("\t**********************************************")
    print("\t**** Greeter - STARTED PYTHON FILE CALL. *****")
    print("\t**********************************************")
    path_finder(path, new_path, dictionary, start, dest)


if __name__ == "__main__":
    # path, new image, building, start=1615, dest=1615
    globals()[sys.argv[1]](sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6])
    # convert()
