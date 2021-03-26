import json
from pixels import bec_dict
from scipy.sparse.csgraph import shortest_path
from scipy.sparse import csr_matrix
from PIL import ImageDraw, Image
import numpy as np
from sknetwork.utils import edgelist2adjacency
import os


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
    # elif building == "pft-1":

    # elif building == "loc-1":

    return edge_list


# calculate shortest path nodes
# draw nodes
# draw a line connecting nodes
def main(path, building, start, dest):

    edge_list = edge_list_getter(building)

    adjacency = edgelist2adjacency(edge_list, undirected=True)
    adjacency = csr_matrix(adjacency)

    dist_matrix, predecessors = shortest_path(adjacency, directed=False, method='auto', return_predecessors=True)

    # grab json data
    with open("D:\Repos\pb-jaw\python\pixels.json") as f:
        data = json.load(f)
    # fill out desired nodes

    print(get_path(predecessors, start, dest))

    nodes = []

    for node in get_path(predecessors, start, dest):
        node = Node(find(data, str(node), 'x1'), find(data, str(node), 'y1'), find(data, str(node), 'x2'),
                    find(data, str(node), 'y2'))
        nodes.append(node)

    img = Image.open(path)
    img.convert("RGB")

    draw = ImageDraw.Draw(img)

    # x center = (x1 + x2) / 2
    # y center = (y1 + y2) / 2
    for i in range(len(nodes)):
        if i == len(nodes) - 1:
            break
        else:
            draw.line([(nodes[i].x1 + nodes[i].x2) / 2, (nodes[i].y1 + nodes[i].y2) / 2,
                       (nodes[i + 1].x1 + nodes[i + 1].x2) / 2, (nodes[i + 1].y1 + nodes[i + 1].y2) / 2],
                      fill="black", width=5)
    img.show()
    img.save(path, "JPEG")


def find(json_object, name, element):
    return [obj for obj in json_object if obj['name'] == name][0][element]


def get_path(predecessors, i, j):
    path = [j]
    k = j
    while predecessors[i, k] != -9999:
        path.append(predecessors[i, k])
        k = predecessors[i, k]
    return path[::-1]


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


class Node:
    def __init__(self, x1, y1, x2, y2, *argv, **kwargs):
        self.x1 = x1
        self.y1 = y1
        self.x2 = x2
        self.y2 = y2


if __name__ == "__main__":
    # path, building, start=1615, dest=1615
    globals()[sys.argv[1]](sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5])
    # main("path", "bec", 1620, 1420)
    # convert()
