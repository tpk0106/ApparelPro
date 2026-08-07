def edit_file(path, old, new, expected=1):
    with open(path, 'rb') as f:
        raw = f.read()
    has_bom = raw.startswith(b'\xef\xbb\xbf')
    if has_bom:
        raw = raw[3:]
    has_crlf = b'\r\n' in raw
    text = raw.decode('utf-8')
    norm = text.replace('\r\n', '\n') if has_crlf else text
    old_n = old.replace('\r\n', '\n')
    new_n = new.replace('\r\n', '\n')
    count = norm.count(old_n)
    if count != expected:
        raise SystemExit(f"[{path}] expected {expected} occurrences of anchor, found {count}")
    norm2 = norm.replace(old_n, new_n)
    out = norm2.replace('\n', '\r\n') if has_crlf else norm2
    out_bytes = out.encode('utf-8')
    if has_bom:
        out_bytes = b'\xef\xbb\xbf' + out_bytes
    with open(path, 'wb') as f:
        f.write(out_bytes)
    print(f"OK: edited {path}")

def check_balance(path):
    with open(path, 'rb') as f:
        raw = f.read()
    if raw.startswith(b'\xef\xbb\xbf'):
        raw = raw[3:]
    text = raw.decode('utf-8')
    bd = text.count('{') - text.count('}')
    pd = text.count('(') - text.count(')')
    print(f"[{path}] braces_diff={bd} parens_diff={pd}")
