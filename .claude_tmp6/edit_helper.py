import io, sys

def edit_file(path, old, new, expected=1):
    with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
        content = f.read()
    uses_crlf = "\r\n" in content
    if uses_crlf:
        old = old.replace("\r\n", "\n").replace("\n", "\r\n")
        new = new.replace("\r\n", "\n").replace("\n", "\r\n")
    count = content.count(old)
    if count != expected:
        print(f"MISMATCH: expected {expected}, found {count} in {path}")
        sys.exit(1)
    content = content.replace(old, new)
    with io.open(path, "w", encoding="utf-8", newline="") as f:
        f.write(content)
    print(f"OK: replaced {count} occurrence(s) in {path}")
