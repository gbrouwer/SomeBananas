import os
import sys

def print_folder_structure(root_dir, indent="", output=sys.stdout):
    entries = sorted(os.listdir(root_dir))
    for idx, entry in enumerate(entries):
        path = os.path.join(root_dir, entry)
        is_last = idx == len(entries) - 1
        prefix = "└── " if is_last else "├── "
        print(indent + prefix + entry, file=output)
        if os.path.isdir(path):
            next_indent = indent + ("    " if is_last else "│   ")
            print_folder_structure(path, next_indent, output)

if __name__ == "__main__":
    root_folder = "../../../Assets/Projects/StoatVsVole"

    # Output to file (UTF-8 encoding) to avoid UnicodeEncodeError
    output_file = "folder_structure.txt"

    with open(output_file, "w", encoding="utf-8") as f:
        print(f"Folder structure for {root_folder}:\n", file=f)
        print(root_folder, file=f)
        print_folder_structure(root_folder, output=f)

    print(f"\n✅ Done! Folder structure written to '{output_file}' (UTF-8 encoded)")
