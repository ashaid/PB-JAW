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
    if building == "locb":
        edge_list = np.array([
            (15, 700, 1), (700, 600, 2), (600, 500, 2), (500, 1100, 2), (1100, 16, 1), (1100, 400, 2),
            (400, 300, 2), (300, 10, 1), (300, 200, 2), (200, 6, 1), (200, 100, 2), (100, 2, 1), 
            (100, 9999, 2), (100, 1000, 2), (1000, 5, 1), (1000, 900, 2), (900, 9, 1), (900, 800, 2),
            (700, 800, 2)
        ])
    # elif building == "pft-1":

    # elif building == "loc-1":

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
    with open(str(pathlib.Path().absolute()) + f"\\python\\" + building + ".json") as f:
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
                        (building_dict[str(start)][2], building_dict[str(start)][3])], (255, 0, 0, 95))
        draw.rectangle([(building_dict[str(dest)][0], building_dict[str(dest)][1]),
                        (building_dict[str(dest)][2], building_dict[str(dest)][3])], (0, 0, 255, 95))

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
