"""Real stdio MCP client for the installed MCP for Blender server."""
import asyncio, json, sys, os, base64
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

async def main():
    env=dict(os.environ)
    params=StdioServerParameters(command='/home/otecglinomes/.cache/uv/archive-v0/cUQxORwnLaUggyK1/bin/mcp-for-blender',env=env)
    async with stdio_client(params) as (read,write):
        async with ClientSession(read,write) as session:
            await session.initialize()
            if sys.argv[1]=='list':
                listing=await session.list_tools()
                print(listing.model_dump_json(indent=2))
                return
            tool=sys.argv[1];args=json.loads(Path(sys.argv[2]).read_text()) if len(sys.argv)>2 else {}
            result=await session.call_tool(tool,args)
            log={'tool':tool,'arguments':args,'isError':result.isError,'content':[]}
            for i,c in enumerate(result.content):
                if c.type=='image':
                    path=Path(__file__).parent/f'mcp-{tool}-{i}.png';path.write_bytes(base64.b64decode(c.data));print('MCP_IMAGE',path)
                    log['content'].append({'type':'image','path':str(path)})
                elif c.type=='text':print(c.text);log['content'].append({'type':'text','text':c.text})
            with (Path(__file__).parent/'mcp-calls.jsonl').open('a') as f:f.write(json.dumps(log)+'\n')
            if result.isError:raise RuntimeError('MCP call failed')
asyncio.run(main())
